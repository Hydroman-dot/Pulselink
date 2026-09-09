using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
namespace HeartbeatEmission {
public class HeartbeatWizard : EditorWindow {
 [Serializable] public class Usage { public Renderer renderer; public int slot; public Material restoreMaterial; public bool selected; public string path; }
 [Serializable] public class MaterialChoice {
  public Material material;
  public List<Usage> usages=new List<Usage>();
  public HeartbeatEffectSetup.Effect effect;
  public Texture mask;
  public Color color=new Color(2.5f,0.025f,0.04f,1);
  public bool expanded;
  public HeartbeatEffectSetup.Target options=new HeartbeatEffectSetup.Target();
  public bool Selected { get { return usages.Exists(u=>u.selected); } }
 }
 [SerializeField] VRCAvatarDescriptor avatar;
 [SerializeField] List<MaterialChoice> materials=new List<MaterialChoice>();
 [SerializeField] int step;
 [SerializeField] string bpm="VRCOSC/Heartrate/Value",mode="HBE/WorldAudioLink",menu="Welt-AudioLink";
 [SerializeField] string search="",status="";
 [SerializeField] bool advanced;
 Vector2 scroll;
 [MenuItem("Tools/Pulselink/Einrichtungs-Wizard")]
 public static void Open(){var w=GetWindow<HeartbeatWizard>();w.titleContent=new GUIContent("Pulselink Wizard");w.minSize=new Vector2(620,520);w.Show();}
 public static void OpenFor(VRCAvatarDescriptor target){Open();var w=GetWindow<HeartbeatWizard>();w.ChooseAvatar(target);}
 static string PathOf(Transform t){return AnimationUtility.CalculateTransformPath(t,null);}
 public static bool IsLil(Material material){return material!=null&&material.shader!=null&&material.shader.name.IndexOf("lilToon",StringComparison.OrdinalIgnoreCase)>=0;}
 public static bool IsSupported(Material material){return IsLil(material)||(material!=null&&material.shader!=null&&material.shader.name.IndexOf("poiyomi",StringComparison.OrdinalIgnoreCase)>=0);}
 public static HeartbeatEffectSetup.Target CopyTarget(HeartbeatEffectSetup.Target source){
  var result=new HeartbeatEffectSetup.Target();foreach(var field in typeof(HeartbeatEffectSetup.Target).GetFields())field.SetValue(result,field.GetValue(source));return result;
 }
 public static HeartbeatEffectSetup.Effect[] EffectsFor(Material material){
  return IsLil(material)?new[]{HeartbeatEffectSetup.Effect.NativeFlow,HeartbeatEffectSetup.Effect.LilUV,HeartbeatEffectSetup.Effect.LilMask,HeartbeatEffectSetup.Effect.LilMaskSpectrum,HeartbeatEffectSetup.Effect.LilPosition,HeartbeatEffectSetup.Effect.UniformPulse,HeartbeatEffectSetup.Effect.SoftBlink,HeartbeatEffectSetup.Effect.RunningLight,HeartbeatEffectSetup.Effect.ColorPulse,HeartbeatEffectSetup.Effect.DoublePulse}:new[]{HeartbeatEffectSetup.Effect.PoiStrengthMultiply,HeartbeatEffectSetup.Effect.PoiStrengthAdd,HeartbeatEffectSetup.Effect.NativeFlow,HeartbeatEffectSetup.Effect.SoftBlink,HeartbeatEffectSetup.Effect.RunningLight,HeartbeatEffectSetup.Effect.ColorPulse,HeartbeatEffectSetup.Effect.DoublePulse};
 }
 public static string EffectName(Material material,HeartbeatEffectSetup.Effect effect){
  switch(effect){
   case HeartbeatEffectSetup.Effect.RunningLight:return "Lauflicht";
   case HeartbeatEffectSetup.Effect.ColorPulse:return "Farbpuls";
   case HeartbeatEffectSetup.Effect.DoublePulse:return "Doppelherzschlag";
   case HeartbeatEffectSetup.Effect.SoftBlink:return "Weiches Herzblinken";
   case HeartbeatEffectSetup.Effect.LilUV:return "UV";
   case HeartbeatEffectSetup.Effect.LilMask:return "Mask";
   case HeartbeatEffectSetup.Effect.LilMaskSpectrum:return "Mask Spectrum";
   case HeartbeatEffectSetup.Effect.LilPosition:return "Position";
   case HeartbeatEffectSetup.Effect.PoiStrengthMultiply:return "Strength Multiply";
   case HeartbeatEffectSetup.Effect.PoiStrengthAdd:return "Strength Add";
   case HeartbeatEffectSetup.Effect.UniformPulse:return IsLil(material)?"Gleichmäßiger Emissionspuls":"Strength Add";
   default:return IsLil(material)?"Rim":"Center Out";
  }
 }
 public static List<MaterialChoice> Scan(VRCAvatarDescriptor target){
  var result=new List<MaterialChoice>();if(target==null)return result;
  var setups=target.GetComponentsInChildren<HeartbeatEffectSetup>(true);
  foreach(var renderer in target.GetComponentsInChildren<Renderer>(true)){
   var slots=renderer.sharedMaterials;
   for(int slot=0;slot<slots.Length;slot++){
    var material=slots[slot];if(material==null)continue;
    HeartbeatEffectSetup.Target existing=null;
    foreach(var setup in setups){existing=setup.targets.Find(t=>t.renderer==renderer&&t.materialSlot==slot);if(existing!=null)break;}
    if(existing!=null&&existing.sourceMaterial!=null)material=existing.sourceMaterial;
    if(!IsSupported(material))continue;
    var choice=result.Find(r=>r.material==material);
    if(choice==null){
     choice=new MaterialChoice{material=material};if(existing!=null){choice.effect=existing.effect;choice.mask=existing.emissionMask;choice.color=existing.pulseColor;choice.options=CopyTarget(existing);}
     result.Add(choice);
    }
    choice.usages.Add(new Usage{renderer=renderer,slot=slot,restoreMaterial=existing!=null?(existing.restoreMaterial!=null?existing.restoreMaterial:existing.sourceMaterial):material,selected=existing!=null,path=AnimationUtility.CalculateTransformPath(renderer.transform,target.transform)});
   }
  }
  return result.OrderBy(r=>r.material.name).ToList();
 }
 void ChooseAvatar(VRCAvatarDescriptor target){
  if(target!=null&&(EditorUtility.IsPersistent(target)||!target.gameObject.scene.IsValid())){status="Bitte einen Avatar aus der Szene wählen, kein Prefab-Asset.";return;}
  avatar=target;materials=Scan(target);step=0;status="";scroll=Vector2.zero;
  bpm="VRCOSC/Heartrate/Value";mode="HBE/WorldAudioLink";menu="Welt-AudioLink";
  if(target!=null){var setup=target.GetComponentInChildren<HeartbeatEffectSetup>(true);if(setup!=null){bpm=setup.bpmParameter;mode=setup.modeParameter;menu=setup.menuLabel;}}
 }
 public static List<HeartbeatEffectSetup.Target> Collect(List<MaterialChoice> rows){
  var result=new List<HeartbeatEffectSetup.Target>();
  foreach(var row in rows)foreach(var usage in row.usages)if(usage.selected)
   {var t=CopyTarget(row.options);t.renderer=usage.renderer;t.materialSlot=usage.slot;t.sourceMaterial=row.material;t.restoreMaterial=usage.restoreMaterial;t.emissionMask=row.mask;t.pulseColor=row.color;t.effect=row.effect;result.Add(t);}
  return result;
 }
 public static HeartbeatEffectSetup Apply(VRCAvatarDescriptor target,List<HeartbeatEffectSetup.Target> targets,string bpm,string mode,string label){
  if(target==null||targets.Count==0)throw new InvalidOperationException("Avatar und mindestens ein Material auswählen.");
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Bitte zuerst den Play Mode beenden.");
  if(EditorUtility.IsPersistent(target)||!target.gameObject.scene.IsValid())throw new InvalidOperationException("Bitte einen Avatar aus der Szene wählen.");
  var all=target.GetComponentsInChildren<HeartbeatEffectSetup>(true);
  if(all.Length>1)throw new InvalidOperationException("Mehrere Heartbeat-Setups auf diesem Avatar. Bitte zuerst im Inspector auf ein Setup zusammenführen.");
  Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Pulselink Wizard");
  HeartbeatEffectSetup setup=all.Length==1?all[0]:null;
  try {
   if(setup==null){
    var asset=AssetDatabase.LoadAssetAtPath<GameObject>(HeartbeatEffectBuilder.Root+"/Pulselink.prefab");
    if(asset==null)throw new InvalidOperationException("Pulselink.prefab fehlt.");
    var go=(GameObject)PrefabUtility.InstantiatePrefab(asset,target.transform);Undo.RegisterCreatedObjectUndo(go,"Create Heartbeat");setup=go.GetComponent<HeartbeatEffectSetup>();
   }
   var oldTargets=new List<HeartbeatEffectSetup.Target>(setup.targets);
   Undo.RecordObject(setup,"Update Heartbeat Selection");setup.targets=targets;setup.bpmParameter=bpm;setup.modeParameter=mode;setup.menuLabel=label;
   HeartbeatEffectBuilder.Build(setup);
   foreach(var previous in oldTargets){
    if(previous.renderer==null||targets.Exists(t=>t.renderer==previous.renderer&&t.materialSlot==previous.materialSlot))continue;
    var slots=previous.renderer.sharedMaterials;if(previous.materialSlot<0||previous.materialSlot>=slots.Length)continue;
    if(slots[previous.materialSlot]==previous.generatedHeartbeat||slots[previous.materialSlot]==previous.generatedAudioLink){
     Undo.RecordObject(previous.renderer,"Restore deselected material");slots[previous.materialSlot]=previous.restoreMaterial!=null?previous.restoreMaterial:previous.sourceMaterial;previous.renderer.sharedMaterials=slots;PrefabUtility.RecordPrefabInstancePropertyModifications(previous.renderer);
    }
   }
   Undo.CollapseUndoOperations(group);return setup;
  } catch {Undo.RevertAllDownToGroup(group);throw;}
 }
 public static int Reset(VRCAvatarDescriptor target,List<HeartbeatEffectSetup.Target> selected=null){
  if(target==null||EditorUtility.IsPersistent(target)||!target.gameObject.scene.IsValid())throw new InvalidOperationException("Einen Avatar aus der Szene auswählen.");
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Bitte zuerst den Play Mode beenden.");
  var all=target.GetComponentsInChildren<HeartbeatEffectSetup>(true);if(all.Length==0)return 0;if(all.Length>1)throw new InvalidOperationException("Mehrere Setups gefunden. Bitte auf ein Setup zusammenführen.");
  var setup=all[0];var removed=setup.targets.Where(t=>selected==null||selected.Exists(s=>s.renderer==t.renderer&&s.materialSlot==t.materialSlot)).ToList();
  if(removed.Count==0)return 0;var remaining=setup.targets.Where(t=>!removed.Contains(t)).Select(CopyTarget).ToList();
  if(remaining.Count>0){Apply(target,remaining,setup.bpmParameter,setup.modeParameter,setup.menuLabel);return removed.Count;}
  Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Reset Heartbeat Effects");
  foreach(var t in removed){
   if(t.renderer==null)continue;var slots=t.renderer.sharedMaterials;if(t.materialSlot<0||t.materialSlot>=slots.Length)continue;
   if(slots[t.materialSlot]!=t.generatedHeartbeat&&slots[t.materialSlot]!=t.generatedAudioLink)continue;
   Undo.RecordObject(t.renderer,"Restore original material");slots[t.materialSlot]=t.restoreMaterial!=null?t.restoreMaterial:t.sourceMaterial;t.renderer.sharedMaterials=slots;PrefabUtility.RecordPrefabInstancePropertyModifications(t.renderer);
  }
  var go=setup.gameObject;bool onlyOwned=go.transform.childCount==0&&go.GetComponents<Component>().All(c=>c is Transform||c is HeartbeatEffectSetup||c is nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator||c is nadena.dev.modular_avatar.core.ModularAvatarParameters||c is nadena.dev.modular_avatar.core.ModularAvatarMenuInstaller);
  if(onlyOwned)Undo.DestroyObjectImmediate(go);
  else{
   foreach(var c in go.GetComponents<Component>())if(c is HeartbeatEffectSetup||c is nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator||c is nadena.dev.modular_avatar.core.ModularAvatarParameters||c is nadena.dev.modular_avatar.core.ModularAvatarMenuInstaller)Undo.DestroyObjectImmediate(c);
  }
  UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(target.gameObject.scene);Undo.CollapseUndoOperations(group);return removed.Count;
 }
 void ResetFromUI(bool selectedOnly){
  try{int count=Reset(avatar,selectedOnly?Collect(materials):null);materials=Scan(avatar);status=count+" Ziel(e) zurückgesetzt. Erzeugte Dateien bleiben erhalten. Szene speichern.";if(!materials.Exists(m=>m.Selected))step=0;}
  catch(Exception e){status=e.Message;Debug.LogException(e);}
 }
 void OnGUI(){
  GUILayout.Space(10);GUILayout.Label("Pulselink",EditorStyles.boldLabel);
  GUILayout.Label("Avatar auswählen. Materialien markieren. Effekt festlegen.",EditorStyles.wordWrappedLabel);
  GUILayout.Space(10);
  using(new EditorGUI.DisabledScope(true))GUILayout.Toolbar(step,new[]{"1  Avatar","2  Materialien","3  Effekte & Einrichten"});
  if(EditorApplication.isPlaying)EditorGUILayout.HelpBox("Zum Einrichten bitte den Play Mode beenden. Die Auswahl kannst du vorbereiten.",MessageType.Info);
  if(!string.IsNullOrEmpty(status))EditorGUILayout.HelpBox(status,MessageType.Info);
  scroll=EditorGUILayout.BeginScrollView(scroll);
  if(step==0)DrawAvatar();else if(step==1)DrawMaterials();else DrawEffects();
  EditorGUILayout.EndScrollView();
  GUILayout.Space(10);
  using(new EditorGUILayout.HorizontalScope()){
   using(new EditorGUI.DisabledScope(step==0))if(GUILayout.Button("Zurück",GUILayout.Height(30))){step--;scroll=Vector2.zero;}
   if(step<2){using(new EditorGUI.DisabledScope(avatar==null||(step==1&&!materials.Exists(m=>m.Selected))))if(GUILayout.Button("Weiter",GUILayout.Height(30))){step++;scroll=Vector2.zero;}}
   else using(new EditorGUI.DisabledScope(EditorApplication.isPlaying||avatar==null||!materials.Exists(m=>m.Selected)))
    if(GUILayout.Button("Effekte einrichten",GUILayout.Height(30))){
     try{var setup=Apply(avatar,Collect(materials),bpm,mode,menu);materials=Scan(avatar);status="Eingerichtet auf "+avatar.name+". Bitte die Szene speichern und den Play Mode zum Test neu starten.";Selection.activeGameObject=setup.gameObject;}
     catch(Exception e){status=e.Message;Debug.LogException(e);}
    }
  }
  GUILayout.Space(8);
 }
 void DrawAvatar(){
  GUILayout.Space(10);GUILayout.Label("Welcher Avatar?",EditorStyles.boldLabel);
  var avatars=UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>(true).Where(a=>a.gameObject.scene.IsValid()&&!EditorUtility.IsPersistent(a)).OrderBy(a=>PathOf(a.transform)).ToArray();
  var labels=new string[avatars.Length+1];labels[0]="Avatar wählen …";int current=0;
  for(int i=0;i<avatars.Length;i++){labels[i+1]=PathOf(avatars[i].transform)+(avatars[i].gameObject.activeInHierarchy?"  [aktiv]":"");if(avatars[i]==avatar)current=i+1;}
  int selected=EditorGUILayout.Popup("Avatar in der Szene",current,labels);if(selected!=current)ChooseAvatar(selected==0?null:avatars[selected-1]);
  var custom=(VRCAvatarDescriptor)EditorGUILayout.ObjectField("Oder Avatar hierher ziehen",avatar,typeof(VRCAvatarDescriptor),true);
  if(custom!=avatar)ChooseAvatar(custom);
  if(avatar==null){EditorGUILayout.HelpBox("Es werden auch deaktivierte Avatare angezeigt.",MessageType.Info);return;}
  GUILayout.Space(12);EditorGUILayout.LabelField("Ausgewählt",PathOf(avatar.transform),EditorStyles.wordWrappedLabel);
  EditorGUILayout.LabelField("Materialien",materials.Count+" unterstützte Materialien gefunden");
  var count=avatar.GetComponentsInChildren<HeartbeatEffectSetup>(true).Length;
  if(count==1)EditorGUILayout.HelpBox("Eine Einrichtung ist vorhanden. Ihre Materialauswahl und Effekte werden geladen.",MessageType.Info);
  if(count==1)using(new EditorGUI.DisabledScope(EditorApplication.isPlaying)){
   EditorGUILayout.HelpBox("Zurücksetzen stellt die Materialien vor der Einrichtung wieder her und entfernt die Effektsteuerung. Rückgängig über Strg+Z.",MessageType.None);
   if(GUILayout.Button("Alle Heartbeat-Effekte dieses Avatars zurücksetzen"))ResetFromUI(false);
  }
  if(count>1)EditorGUILayout.HelpBox("Mehrere Einrichtungen gefunden. Vor dem Anwenden bitte auf ein Setup zusammenführen.",MessageType.Warning);
 }
 void DrawMaterials(){
  GUILayout.Space(10);GUILayout.Label("Welche Materialien sollen reagieren?",EditorStyles.boldLabel);
  search=EditorGUILayout.TextField("Suchen",search);
  using(new EditorGUILayout.HorizontalScope()){
   if(GUILayout.Button("Alle bearbeitbaren auswählen"))foreach(var row in materials)if(!row.material.shader.name.StartsWith("Hidden/Locked/"))foreach(var u in row.usages)u.selected=true;
   if(GUILayout.Button("Auswahl leeren"))foreach(var row in materials)foreach(var u in row.usages)u.selected=false;
  }
  if(materials.Count==0)EditorGUILayout.HelpBox("Keine lilToon- oder Poiyomi-Materialien gefunden.",MessageType.Info);
  foreach(var row in materials){
   if(row.material==null)continue;
   if(!string.IsNullOrEmpty(search)&&row.material.name.IndexOf(search,StringComparison.OrdinalIgnoreCase)<0&&!row.usages.Exists(u=>u.path.IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0))continue;
   using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox)){
    bool locked=row.material.shader.name.StartsWith("Hidden/Locked/");
    using(new EditorGUILayout.HorizontalScope()){
     using(new EditorGUI.DisabledScope(locked)){
      bool active=EditorGUILayout.Toggle(row.Selected,GUILayout.Width(20));if(active!=row.Selected)foreach(var u in row.usages)u.selected=active;
     }
     GUILayout.Label(AssetPreview.GetMiniThumbnail(row.material),GUILayout.Width(25),GUILayout.Height(25));
     GUILayout.Label(row.material.name,EditorStyles.boldLabel);
     if(GUILayout.Button("Zeigen",GUILayout.Width(65)))EditorGUIUtility.PingObject(row.material);
    }
    GUILayout.Label((IsLil(row.material)?"lilToon":"Poiyomi")+" · "+row.usages.Count+" Material-Slot(s)",EditorStyles.miniLabel);
    if(locked)EditorGUILayout.HelpBox("Gesperrtes Poiyomi-Material. Erst eine entsperrte Kopie am Avatar zuweisen und neu laden.",MessageType.Info);
    row.expanded=EditorGUILayout.Foldout(row.expanded,"Verwendung am Avatar",true);
    if(row.expanded)foreach(var use in row.usages)using(new EditorGUI.DisabledScope(locked))use.selected=EditorGUILayout.ToggleLeft(use.path+"  · Slot "+use.slot,use.selected);
   }
  }
  if(avatar!=null&&avatar.GetComponentInChildren<HeartbeatEffectSetup>(true)!=null)using(new EditorGUI.DisabledScope(EditorApplication.isPlaying||!materials.Exists(m=>m.Selected)))if(GUILayout.Button("Ausgewählte Effekte zurücksetzen"))ResetFromUI(true);
  if(GUILayout.Button("Materialliste neu laden")){materials=Scan(avatar);status="Materialliste neu geladen; vorhandene Einrichtung übernommen.";}
 }
 public static string ExistingEffectDescription(bool lil,HeartbeatEffectSetup.Effect effect){
  switch(effect){
   case HeartbeatEffectSetup.Effect.NativeFlow:return lil?"Blickwinkelabhängiger Leuchtverlauf über die markierten Flächen und Kanten. Ausdehnung und Versatz verändern den Verlauf. Im Herzpuls-Modus läuft eine Pulskurve darüber; Welt-AudioLink steuert ihn mit dem gewählten Audioband.":"Center Out: Ein blickwinkelabhängiger Leuchtverlauf bewegt sich über die markierte Oberfläche. Das Herzpuls-Tempo folgt den BPM. Im Welt-AudioLink-Modus bestimmen Audioband, Dauer und Intensitätsschwelle die Reaktion. Der Ursprung ist kein fester Punkt im Raum.";
   case HeartbeatEffectSetup.Effect.UniformPulse:return lil?"Alle markierten Leuchtflächen werden gleichzeitig kurz heller und wieder dunkel. Das Tempo folgt den BPM. Im Welt-AudioLink-Modus folgt die Helligkeit dem gewählten Audioband.":"Die Emission steigt mit jedem Puls über die Grundstärke an und klingt wieder ab. Im Welt-AudioLink-Modus wird die Musikstärke zur Grundstärke addiert.";
   case HeartbeatEffectSetup.Effect.LilUV:return "Der Puls wandert entlang der UV-Koordinaten über die markierten Flächen. UV-Winkel, Ausdehnung und Versatz bestimmen Richtung und Verlauf. UV-Nähte können ihn unterbrechen. Welt-AudioLink nutzt denselben räumlichen Verlauf mit Musikdaten.";
   case HeartbeatEffectSetup.Effect.LilMask:return "Eine zusätzliche Steuermaske legt fest, wann und wie stark einzelne Stellen leuchten. Im Herzpuls-Modus verschiebt sie den Puls pro Stelle; im Welt-AudioLink-Modus wählt sie zusätzlich das Audioband. Die Emissionsmap begrenzt weiterhin die Leuchtflächen.";
   case HeartbeatEffectSetup.Effect.LilMaskSpectrum:return "Im Welt-AudioLink-Modus reagieren Stellen der Steuermaske auf unterschiedliche Frequenzen und Pegel. Im Herzpuls-Modus werden die R-Pegelschwellen gegen eine Pulskurve ausgewertet; die Herzfrequenz liefert kein echtes Musikspektrum. Eine zusätzliche Steuermaske ist erforderlich.";
   case HeartbeatEffectSetup.Effect.LilPosition:return "Der Leuchtverlauf richtet sich nach dem Abstand zu einem einstellbaren Ursprung im jeweiligen Renderer-Objekt. So entstehen nach außen oder innen laufende Lichtwellen. Ausdehnung und Versatz bestimmen den Verlauf; Welt-AudioLink verwendet dieselbe räumliche Zuordnung.";
   case HeartbeatEffectSetup.Effect.PoiStrengthMultiply:return "Die Grundstärke wird mit einem pulsierenden Multiplikator zwischen Min und Max verrechnet. Im Welt-AudioLink-Modus liefert das gewählte Audioband den Multiplikator. Bei Grundstärke null bleibt der Effekt dunkel; ohne AudioLink-Quelle bleibt im Welt-Modus die Grundstärke sichtbar.";
   case HeartbeatEffectSetup.Effect.PoiStrengthAdd:return "Ein pulsierender Wert zwischen Min und Max wird zur Grundstärke addiert. Im Welt-AudioLink-Modus liefert das gewählte Audioband diesen Zusatz. Grundstärke null ermöglicht eine Dunkelpause; eine höhere Grundstärke sorgt für dauerhaftes Grundleuchten.";
   default:return null;
  }
 }
 void DrawEffectOptions(MaterialChoice row){
  var t=row.options;bool lil=IsLil(row.material);
  var description=ExistingEffectDescription(lil,row.effect);if(!string.IsNullOrEmpty(description))EditorGUILayout.HelpBox(description,MessageType.Info);
  if(row.effect==HeartbeatEffectSetup.Effect.RunningLight){
   EditorGUILayout.HelpBox("Ein weicher Leuchtstreifen wandert entlang der UVs durch die markierten Flächen. Eine Wiederholung pro BPM-Zyklus. UV-Nähte können den Streifen unterbrechen. Welt-AudioLink: lilToon nutzt räumlich verzögerte Musik; Poiyomi lässt die markierten Flächen zur Musik leuchten.",MessageType.Info);
   t.flowAxis=EditorGUILayout.Popup("Richtung",t.flowAxis,new[]{"U / horizontal","V / vertikal"});t.flowRepeat=Mathf.Max(0.1f,EditorGUILayout.FloatField("Streifen-Wiederholungen",t.flowRepeat<=0?1:t.flowRepeat));
  }
  if(row.effect==HeartbeatEffectSetup.Effect.ColorPulse){
   EditorGUILayout.HelpBox("Die Leuchtflächen wechseln weich von der Ruhefarbe zur Pulsfarbe und zurück. Die HDR-Farben bestimmen auch die Helligkeit. Welt-AudioLink nutzt die Pulsfarbe mit Musikhelligkeit.",MessageType.Info);
   t.restingColor=EditorGUILayout.ColorField(new GUIContent("Ruhefarbe (HDR)"),t.restingColor,true,true,true);
  }
  if(row.effect==HeartbeatEffectSetup.Effect.DoublePulse)EditorGUILayout.HelpBox("Zwei weiche Lichtimpulse pro BPM-Zyklus: ein kräftiger erster und ein schwächerer zweiter Impuls, danach Pause. Ein stilisierter Doppelherzschlag. Welt-AudioLink folgt dem Audioband.",MessageType.Info);
  if(row.effect==HeartbeatEffectSetup.Effect.SoftBlink)EditorGUILayout.HelpBox("Sanfter Anstieg, längeres Abklingen und kurze Dunkelpause. Das Tempo folgt den BPM. Welt-AudioLink reagiert auf das ausgewählte Audioband.",MessageType.Info);
  if(lil){
   bool mask=row.effect==HeartbeatEffectSetup.Effect.LilMask||row.effect==HeartbeatEffectSetup.Effect.LilMaskSpectrum;
   if(mask){
    t.audioLinkMask=(Texture)EditorGUILayout.ObjectField("AudioLink-Steuermaske",t.audioLinkMask,typeof(Texture),false);
    t.audioLinkMaskUV=EditorGUILayout.Popup("Masken-UV",t.audioLinkMaskUV,new[]{"UV0","UV1","UV2","UV3"});
    EditorGUILayout.HelpBox(row.effect==HeartbeatEffectSetup.Effect.LilMask?"Steuermaske: R = Verzögerung, G = Band, B = Stärke. Die Emissionsmap darunter begrenzt separat die Leuchtflächen.":"Steuermaske: R = Pegel, G = Frequenz, B = Stärke. Herzpuls nutzt R-Pegelschwellen; ein echtes Frequenzspektrum gibt es nur mit Welt-AudioLink.",MessageType.None);
    if(t.audioLinkMask==null)EditorGUILayout.HelpBox("Für diesen Effekt eine AudioLink-Steuermaske zuweisen.",MessageType.Warning);
   }else t.audioBand=EditorGUILayout.Popup("AudioLink-Band",Mathf.Clamp(t.audioBand,0,3),new[]{"Bass","Low Mid","High Mid","Treble"});
   if(row.effect==HeartbeatEffectSetup.Effect.LilMaskSpectrum){
    t.spectrumGain=Mathf.Max(0,EditorGUILayout.FloatField("Spektrum-Verstärkung",t.spectrumGain));t.spectrumThreshold=EditorGUILayout.Slider("Spektrum-Breite / Schwelle",t.spectrumThreshold,0.01f,1);
   }else if(!mask&&row.effect!=HeartbeatEffectSetup.Effect.SoftBlink&&(int)row.effect<9){
    t.spatialScale=EditorGUILayout.FloatField("Räumliche Ausdehnung",t.spatialScale);t.spatialOffset=EditorGUILayout.FloatField("Versatz",t.spatialOffset);
    if(row.effect==HeartbeatEffectSetup.Effect.LilUV)t.uvAngle=EditorGUILayout.Slider("UV-Winkel",t.uvAngle,-180,180);
    if(row.effect==HeartbeatEffectSetup.Effect.LilPosition)t.positionOrigin=EditorGUILayout.Vector3Field("Ursprung am Objekt",t.positionOrigin);
   }
  }else{
   t.audioBand=EditorGUILayout.Popup("AudioLink-Band",Mathf.Clamp(t.audioBand,0,4),new[]{"Bass","Low Mid","High Mid","Treble","Volume"});
   if(row.effect!=HeartbeatEffectSetup.Effect.RunningLight&&row.effect!=HeartbeatEffectSetup.Effect.ColorPulse)t.strengthRange=EditorGUILayout.Vector2Field(row.effect==HeartbeatEffectSetup.Effect.PoiStrengthMultiply?"Multiplikator Min / Max":"Stärke Min / Max",t.strengthRange);
   if(row.effect==HeartbeatEffectSetup.Effect.PoiStrengthMultiply){
    t.multiplyBase=Mathf.Max(0,EditorGUILayout.FloatField("Grundstärke",t.multiplyBase));EditorGUILayout.HelpBox("Die Grundstärke wird mit dem Signal multipliziert. Ohne AudioLink-Quelle bleibt im Welt-Modus die Grundstärke sichtbar.",MessageType.None);
   }else if(row.effect!=HeartbeatEffectSetup.Effect.NativeFlow&&row.effect!=HeartbeatEffectSetup.Effect.ColorPulse&&row.effect!=HeartbeatEffectSetup.Effect.RunningLight)t.addBase=Mathf.Max(0,EditorGUILayout.FloatField("Grundstärke",t.addBase));
   else if(row.effect==HeartbeatEffectSetup.Effect.NativeFlow){t.centerOutDuration=EditorGUILayout.Slider("Center-Out-Dauer",t.centerOutDuration,-1,1);t.centerOutThreshold=EditorGUILayout.Slider("Intensitätsschwelle",t.centerOutThreshold,0,1);}
  }
 }
 void DrawEffects(){
  GUILayout.Space(10);GUILayout.Label("Welchen Effekt pro Material?",EditorStyles.boldLabel);
  foreach(var row in materials)if(row.Selected){
   using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox)){
    GUILayout.Label(row.material.name,EditorStyles.boldLabel);
    var effects=EffectsFor(row.material);int current=Array.IndexOf(effects,row.effect);if(current<0)current=Array.IndexOf(effects,HeartbeatEffectSetup.Effect.PoiStrengthAdd);int picked=EditorGUILayout.Popup("Effekt",Mathf.Max(0,current),effects.Select(e=>EffectName(row.material,e)).ToArray());if(picked!=current)row.effect=effects[picked];
    DrawEffectOptions(row);

    row.mask=(Texture)EditorGUILayout.ObjectField("Emissionsmap / Maske",row.mask,typeof(Texture),false);
    if(row.mask==null)GUILayout.Label("Automatisch: vorhandene Emissionsmap verwenden.",EditorStyles.miniLabel);
    row.color=EditorGUILayout.ColorField(new GUIContent("Pulsfarbe (HDR)"),row.color,true,true,true);
    GUILayout.Label(row.usages.Count(u=>u.selected)+" ausgewählte Material-Slots",EditorStyles.miniLabel);
   }
  }
  advanced=EditorGUILayout.Foldout(advanced,"Signal und Menü",true);
  if(advanced){bpm=EditorGUILayout.TextField("BPM-Parameter",bpm);mode=EditorGUILayout.TextField("AudioLink-Toggle",mode);menu=EditorGUILayout.TextField("Menübezeichnung",menu);}
  GUILayout.Space(10);
  int count=materials.Sum(row=>row.usages.Count(u=>u.selected));
  EditorGUILayout.HelpBox("Anwenden auf "+(avatar!=null?avatar.name:"—")+": "+materials.Count(row=>row.Selected)+" Materialien, "+count+" Slots.\nEigene Materialkopien und Menü werden erzeugt. Aus = Herzpuls; An = Welt-AudioLink.",MessageType.Info);
  if(avatar!=null&&avatar.GetComponentInChildren<HeartbeatEffectSetup>(true)!=null)GUILayout.Label("Abgewählte bisherige Ziele werden auf ihr Quellmaterial zurückgesetzt.",EditorStyles.wordWrappedMiniLabel);
 }
}
}