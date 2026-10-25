using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using nadena.dev.modular_avatar.core;
namespace HeartbeatEmission {
[CustomEditor(typeof(HeartbeatEffectSetup))]
public class HeartbeatEffectSetupEditor : Editor {
 public override void OnInspectorGUI() {
  EditorGUILayout.HelpBox("Unter den Avatar ziehen, Ziele hinzufügen und Einrichten drücken. Aus = Herzpuls, An = Welt-AudioLink. Originalmaterialien bleiben unverändert. lilToon nutzt Emission 2; Poiyomi einen freien Emissionskanal.",MessageType.Info);
  if(GUILayout.Button("Im Einrichtungs-Wizard öffnen")) HeartbeatWizard.OpenFor(((HeartbeatEffectSetup)target).GetComponentInParent<VRCAvatarDescriptor>(true));
  DrawDefaultInspector();
  var setup=(HeartbeatEffectSetup)target;
  using(new EditorGUI.DisabledScope(EditorApplication.isPlaying)) {
   if(GUILayout.Button("Effekt einrichten / aktualisieren",GUILayout.Height(30))) {
    try { HeartbeatEffectBuilder.Build(setup); EditorUtility.DisplayDialog("Pulselink",setup.lastBuildSummary,"OK"); }
    catch(Exception e) { Debug.LogException(e); EditorUtility.DisplayDialog("Pulselink",e.Message,"OK"); }
   }
  }
  if(!string.IsNullOrEmpty(setup.generatedFolder)) {
   EditorGUILayout.HelpBox(setup.lastBuildSummary+"\n"+setup.generatedFolder,MessageType.None);
   if(GUILayout.Button("Erzeugte Dateien anzeigen"))EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(setup.generatedFolder));
  }
 }
}
public static class HeartbeatEffectBuilder {
 public const string Root="Assets/Pulselink";
 class Plan { public HeartbeatEffectSetup.Target target; public Material source; public bool lil; public int channel; public string path; public Texture mask; public string maskProperty; public Material heart,world; }
 static void SetupLil(Material material) {
  foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()){
   var type=assembly.GetType("lilToon.lilMaterialUtils");
   if(type==null)continue;
   var method=type.GetMethod("SetupMultiMaterial",new[]{typeof(Material)});
   if(method!=null){method.Invoke(null,new object[]{material});return;}
  }
  throw new InvalidOperationException("lilToon Material-Setup nicht gefunden.");
 }
 // One BPM-scaled cycle: gentle rise, longer fade, then a short dark pause.
 public static AnimationCurve SoftBlinkCurve(float min,float max){
  return new AnimationCurve(new Keyframe(0,min,0,0),new Keyframe(10.8f,max,0,0),new Keyframe(51,min,0,0),new Keyframe(60,min,0,0));
 }
 public static AnimationCurve DoublePulseCurve(float min,float max){
  float second=Mathf.Lerp(min,max,0.65f);
  return new AnimationCurve(new Keyframe(0,min,0,0),new Keyframe(6,max,0,0),new Keyframe(15,min,0,0),new Keyframe(22.2f,second,0,0),new Keyframe(36,min,0,0),new Keyframe(60,min,0,0));
 }
 static bool IsExtra(HeartbeatEffectSetup.Effect e){return e==HeartbeatEffectSetup.Effect.RunningLight||e==HeartbeatEffectSetup.Effect.ColorPulse||e==HeartbeatEffectSetup.Effect.DoublePulse;}
 static Texture2D WaveTexture(AnimationCurve curve,string path,bool vertical=false){
  int width=256,height=vertical?256:4;var tex=new Texture2D(width,height,TextureFormat.RGBA32,false,true){name=Path.GetFileNameWithoutExtension(path),wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
  var pixels=new Color[width*height];for(int y=0;y<height;y++)for(int x=0;x<width;x++){float value=curve.Evaluate((vertical?y:x)/256f*60);pixels[y*width+x]=new Color(value,value,value,1);}tex.SetPixels(pixels);tex.Apply();AssetDatabase.CreateAsset(tex,path);return tex;
 }
 static void ColorCurves(AnimationClip clip,Plan p,string property,Color from,Color to){
  var curve=SoftBlinkCurve(0,1);string[] channels={"r","g","b","a"};
  for(int c=0;c<4;c++){var keys=curve.keys;for(int k=0;k<keys.Length;k++)keys[k].value=Mathf.LerpUnclamped(from[c],to[c],keys[k].value);FloatCurve(clip,p,property+"."+channels[c],new AnimationCurve(keys));}
 }
 static bool IsCenter(Plan p){return p.target.effect==HeartbeatEffectSetup.Effect.NativeFlow;}
 static bool IsMultiply(Plan p){return p.target.effect==HeartbeatEffectSetup.Effect.PoiStrengthMultiply;}
 static int LilMode(HeartbeatEffectSetup.Effect e){
  switch(e){case HeartbeatEffectSetup.Effect.RunningLight:return 2;case HeartbeatEffectSetup.Effect.ColorPulse:case HeartbeatEffectSetup.Effect.DoublePulse:case HeartbeatEffectSetup.Effect.SoftBlink:case HeartbeatEffectSetup.Effect.UniformPulse:return 0;case HeartbeatEffectSetup.Effect.LilUV:return 2;case HeartbeatEffectSetup.Effect.LilMask:return 3;case HeartbeatEffectSetup.Effect.LilMaskSpectrum:return 4;case HeartbeatEffectSetup.Effect.LilPosition:return 5;default:return 1;}
 }
 static void SpectrumAssets(Texture source,float gain,float threshold,string output,out Texture2D mask,out Texture2D table){
  int width=Mathf.Min(source.width,2048),height=Mathf.Min(source.height,2048);
  var old=RenderTexture.active;var rt=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
  mask=new Texture2D(width,height,TextureFormat.RGBA32,false,true){name="Heartbeat Spectrum Mask",wrapMode=TextureWrapMode.Repeat};
  try{Graphics.Blit(source,rt);RenderTexture.active=rt;mask.ReadPixels(new Rect(0,0,width,height),0,0);mask.Apply();}
  finally{RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);}
  var pixels=mask.GetPixels();for(int i=0;i<pixels.Length;i++)pixels[i]=new Color(0,(pixels[i].r*255f+0.5f)/256f,pixels[i].b,1);mask.SetPixels(pixels);mask.Apply();
  table=new Texture2D(256,256,TextureFormat.RGBA32,false,true){name="Heartbeat Spectrum Thresholds",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
  var data=new Color[256*256];for(int x=0;x<256;x++){float d=Mathf.Abs(x/256f-0.5f);float amp=(d<0.1f?0.5f+0.5f*Mathf.Cos(d/0.1f*Mathf.PI):0)*gain;
   for(int y=0;y<256;y++){float level=y/255f;float v=(threshold<1?Mathf.Abs(amp-level)<threshold:amp>level)?1:0;data[y*256+x]=new Color(v,v,v,1);}}
  table.SetPixels(data);table.Apply();
  AssetDatabase.CreateAsset(mask,AssetDatabase.GenerateUniqueAssetPath(output+"/HeartbeatSpectrumMask.asset"));AssetDatabase.CreateAsset(table,AssetDatabase.GenerateUniqueAssetPath(output+"/HeartbeatSpectrumTable.asset"));
 }
 static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
 static string Safe(string s){foreach(char c in Path.GetInvalidFileNameChars())s=s.Replace(c,'_');return s;}
 static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;Folder(Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'),Path.GetFileName(path));}
 static void FloatCurve(AnimationClip clip,Plan p,string property,AnimationCurve curve) {
  AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(p.path,p.target.renderer.GetType(),"material."+property),curve);
 }
 static void Constant(AnimationClip clip,Plan p,string property,float value){FloatCurve(clip,p,property,AnimationCurve.Constant(0,1f/60f,value));}
 static void Swap(AnimationClip clip,Plan p,Material material){
  AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve(p.path,p.target.renderer.GetType(),"m_Materials.Array.data["+p.target.materialSlot+"]"),new[]{new ObjectReferenceKeyframe{time=0,value=material}});
 }
 static AnimationClip Clip(string folder,string name){var c=new AnimationClip{name=name,frameRate=60};AssetDatabase.CreateAsset(c,folder+"/"+name+".anim");return c;}
 static Material Copy(Material source,string folder,string name) {
  // Poiyomi uses material names in generated shader paths; keep them short on Windows.
  if(source.shader!=null&&source.shader.name.IndexOf("poiyomi",StringComparison.OrdinalIgnoreCase)>=0)name="PL_"+Guid.NewGuid().ToString("N").Substring(0,8)+(name.EndsWith("_WorldAudioLink",StringComparison.Ordinal)?"_AL":"_HR");
  var material=new Material(source){name=name};AssetDatabase.CreateAsset(material,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+Safe(name)+".mat"));return material;
 }
 static string PoiProperty(string prefix,int channel){return prefix+(channel==0?"":channel.ToString());}
 static string FindMask(Material source,bool lil){
  if(lil&&source.GetFloat("_UseEmission2nd")>0)foreach(var p in new[]{"_Emission2ndBlendMask","_Emission2ndMap"})if(source.GetTexture(p)!=null)return p;
  foreach(var p in lil?new[]{"_EmissionBlendMask","_EmissionMap"}:new[]{"_EmissionMap","_EmissionMask"})if(source.HasProperty(p)&&source.GetTexture(p)!=null)return p;
  return null;
 }
 static void Transition(AnimatorState a,AnimatorState b,string parameter,bool on){var t=a.AddTransition(b);t.hasExitTime=false;t.duration=0;t.AddCondition(on?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,parameter);}
 public static void Build(HeartbeatEffectSetup setup) {
  Require(!EditorApplication.isPlaying,"Bitte zuerst den Play Mode beenden.");
  Require(!EditorUtility.IsPersistent(setup),"Das Prefab zuerst unter einen Avatar in der Szene ziehen.");
  var avatar=setup.GetComponentInParent<VRCAvatarDescriptor>(true);
  Require(avatar!=null,"Kein VRChat-Avatar über dem Prefab gefunden.");
  Require(setup.targets!=null&&setup.targets.Count>0,"Mindestens einen Renderer hinzufügen.");
  Require(!string.IsNullOrWhiteSpace(setup.bpmParameter)&&!string.IsNullOrWhiteSpace(setup.modeParameter)&&setup.bpmParameter!=setup.modeParameter,"Zwei unterschiedliche Parameternamen angeben.");
  var plans=new List<Plan>();var used=new HashSet<string>();var channels=new Dictionary<Renderer,int>();
  foreach(var target in setup.targets) {
   Require(target!=null&&target.renderer!=null,"Ein Ziel hat keinen Renderer.");
   Require(target.renderer.transform.IsChildOf(avatar.transform),"Alle Renderer müssen zu diesem Avatar gehören.");
   var materials=target.renderer.sharedMaterials;
   Require(target.materialSlot>=0&&target.materialSlot<materials.Length,"Ungültiger Material-Slot bei "+target.renderer.name);
   var source=target.sourceMaterial!=null?target.sourceMaterial:materials[target.materialSlot];
   Require(source!=null,"Material fehlt bei "+target.renderer.name);
   bool lil=source.shader.name.IndexOf("lilToon",StringComparison.OrdinalIgnoreCase)>=0;
   bool poi=source.shader.name.IndexOf("poiyomi",StringComparison.OrdinalIgnoreCase)>=0;
   Require(lil||poi,"Unterstützt werden lilToon und Poiyomi: "+source.name);
   Require(!source.shader.name.StartsWith("Hidden/Locked/"),"Poiyomi-Material "+source.name+" ist gesperrt. Bitte eine entsperrte Kopie als Quellmaterial zuweisen.");
   Require(!lil||source.HasProperty("_AudioLinkAsLocal"),"Diese lilToon-Variante unterstützt den lokalen AudioLink-Modus nicht.");
   Require(IsExtra(target.effect)||(lil?((int)target.effect<=5||target.effect==HeartbeatEffectSetup.Effect.SoftBlink):target.effect==HeartbeatEffectSetup.Effect.SoftBlink||target.effect==HeartbeatEffectSetup.Effect.NativeFlow||target.effect==HeartbeatEffectSetup.Effect.UniformPulse||target.effect==HeartbeatEffectSetup.Effect.PoiStrengthMultiply||target.effect==HeartbeatEffectSetup.Effect.PoiStrengthAdd),"Effekt passt nicht zum Shader: "+source.name);
   if(lil&&(target.effect==HeartbeatEffectSetup.Effect.LilMask||target.effect==HeartbeatEffectSetup.Effect.LilMaskSpectrum))Require(target.audioLinkMask!=null,"AudioLink-Maske fehlt: "+source.name);
   Require(target.audioBand>=0&&target.audioBand<=(lil?3:4),"Ungültiges AudioLink-Band.");
   if(target.effect==HeartbeatEffectSetup.Effect.RunningLight)Require(target.flowAxis>=0&&target.flowAxis<=1&&target.flowRepeat>0,"Lauflicht benötigt U/V-Richtung und Ausdehnung größer null.");
   string key=target.renderer.GetInstanceID()+":"+target.materialSlot;Require(used.Add(key),"Material-Slot doppelt eingetragen.");
   var plan=new Plan{target=target,source=source,lil=lil,path=AnimationUtility.CalculateTransformPath(target.renderer.transform,avatar.transform)};
   if(poi) {
    plan.channel=0;
    for(int ch=1;ch<=3;ch++)if(source.HasProperty("_EnableEmission"+ch)&&source.GetFloat("_EnableEmission"+ch)==0){plan.channel=ch;break;}
    Require(plan.channel>0,"Kein freier zusätzlicher Poiyomi-Emissionskanal: "+source.name);
    
   }
   plan.maskProperty=FindMask(source,lil);plan.mask=target.emissionMask!=null?target.emissionMask:plan.maskProperty==null?null:source.GetTexture(plan.maskProperty);
   Require(plan.mask!=null,"Keine Emissionsmap gefunden: Bitte für "+target.renderer.name+" eine Maske auswählen.");
   plans.Add(plan);
  }
  foreach(var plan in plans)if(!plan.lil&&!channels.ContainsKey(plan.target.renderer)){
   int common=0;for(int ch=1;ch<=3;ch++){bool free=true;foreach(var other in plans)if(!other.lil&&other.target.renderer==plan.target.renderer)free &= other.source.HasProperty("_EnableEmission"+ch)&&other.source.GetFloat("_EnableEmission"+ch)==0;if(free){common=ch;break;}}
   Require(common>0,"Kein gemeinsamer freier Poiyomi-Emissionskanal auf "+plan.target.renderer.name);channels[plan.target.renderer]=common;
  }
  foreach(var plan in plans)if(!plan.lil)plan.channel=channels[plan.target.renderer];
  foreach(var p in plans)if(!p.lil)foreach(var other in plans)if(!other.lil&&other.target.renderer==p.target.renderer)Require(other.target.effect==p.target.effect&&other.target.strengthRange==p.target.strengthRange&&other.target.multiplyBase==p.target.multiplyBase&&other.target.addBase==p.target.addBase,"Poiyomi-Slots desselben Renderers benötigen denselben Effekt und dieselben Stärke-Werte.");
  foreach(var p in plans)foreach(var other in plans)if(p!=other&&p.target.renderer==other.target.renderer){
   if(p.target.effect==HeartbeatEffectSetup.Effect.ColorPulse||other.target.effect==HeartbeatEffectSetup.Effect.ColorPulse)
    Require(p.target.effect==other.target.effect&&p.target.pulseColor==other.target.pulseColor&&p.target.restingColor==other.target.restingColor,"Farbpuls-Slots desselben Renderers benötigen identische Farben und denselben Effekt.");
   if(!p.lil&&p.target.effect==HeartbeatEffectSetup.Effect.RunningLight)
    Require(p.target.flowAxis==other.target.flowAxis&&p.target.flowRepeat==other.target.flowRepeat,"Lauflicht-Slots desselben Poiyomi-Renderers benötigen dieselbe Richtung und Ausdehnung.");
  }
  Folder(Root+"/Generated");
  var output=AssetDatabase.GenerateUniqueAssetPath(Root+"/Generated/"+Safe(avatar.name)+"_"+DateTime.Now.ToString("yyyyMMdd_HHmmss"));
  Folder(output);Folder(output+"/Materials");
  var pulseTex=new Texture2D(256,4,TextureFormat.RGBA32,false,true){name="Heartbeat Pulse",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
  for(int x=0;x<256;x++){float distance=Mathf.Abs(x/256f-0.5f);float v=distance<0.1f?0.5f+0.5f*Mathf.Cos(distance/0.1f*Mathf.PI):0;for(int y=0;y<4;y++)pulseTex.SetPixel(x,y,new Color(v,v,v,1));}pulseTex.Apply();
  AssetDatabase.CreateAsset(pulseTex,output+"/HeartbeatPulse.asset");
  Texture2D softTex=null;
  if(plans.Exists(p=>p.lil&&p.target.effect==HeartbeatEffectSetup.Effect.SoftBlink)){
   softTex=new Texture2D(256,4,TextureFormat.RGBA32,false,true){name="Pulselink Soft Blink",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
   var curve=SoftBlinkCurve(0,1);
   for(int x=0;x<256;x++){float v=curve.Evaluate(x/256f*60);for(int y=0;y<4;y++)softTex.SetPixel(x,y,new Color(v,v,v,1));}
   softTex.Apply();AssetDatabase.CreateAsset(softTex,output+"/SoftBlink.asset");
  }
  Texture2D doubleTex=null,runHorizontal=null,runVertical=null;
  if(plans.Exists(p=>p.lil&&p.target.effect==HeartbeatEffectSetup.Effect.DoublePulse))doubleTex=WaveTexture(DoublePulseCurve(0,1),output+"/DoublePulse.asset");
  if(plans.Exists(p=>!p.lil&&p.target.effect==HeartbeatEffectSetup.Effect.RunningLight&&p.target.flowAxis==0))runHorizontal=WaveTexture(new AnimationCurve(new Keyframe(0,0),new Keyframe(24,0),new Keyframe(30,1),new Keyframe(36,0),new Keyframe(60,0)),output+"/RunningU.asset");
  if(plans.Exists(p=>!p.lil&&p.target.effect==HeartbeatEffectSetup.Effect.RunningLight&&p.target.flowAxis==1))runVertical=WaveTexture(new AnimationCurve(new Keyframe(0,0),new Keyframe(24,0),new Keyframe(30,1),new Keyframe(36,0),new Keyframe(60,0)),output+"/RunningV.asset",true);
  var controller=AnimatorController.CreateAnimatorControllerAtPath(output+"/Heartbeat.controller");
  controller.AddParameter(setup.bpmParameter,AnimatorControllerParameterType.Float);
  controller.AddParameter(setup.modeParameter,AnimatorControllerParameterType.Bool);
  var heartMode=Clip(output,"Mode_Heartbeat");var worldMode=Clip(output,"Mode_WorldAudioLink");
  var extras=Clip(output,"Additional_Heartbeat");var extrasWorld=Clip(output,"Additional_WorldAudioLink");
  var lilPulse=Clip(output,"lilToon_Heartbeat");
  var uniform=Clip(output,"Poiyomi_UniformHeartbeat");var uniformWorld=Clip(output,"Poiyomi_UniformWorld");
  var low=Clip(output,"Poiyomi_BPM0");var one=Clip(output,"Poiyomi_BPM1");var high=Clip(output,"Poiyomi_BPM255");var poiWorld=Clip(output,"Poiyomi_WorldAudioLink");
  foreach(var p in plans) {
   p.heart=Copy(p.source,output+"/Materials",p.source.name+"_Heartbeat");
   var m=p.heart;
   if(p.lil) {
    m.SetFloat("_UseEmission2nd",1);m.SetColor("_Emission2ndColor",p.target.pulseColor);m.SetFloat("_Emission2ndBlend",1);
    // Reuse the authored second-emission maps when no explicit replacement was requested.
    if(p.target.emissionMask!=null||p.source.GetFloat("_UseEmission2nd")==0){
     m.SetTexture("_Emission2ndMap",null);m.SetTexture("_Emission2ndBlendMask",p.mask);
     if(p.maskProperty!=null){m.SetTextureScale("_Emission2ndBlendMask",p.source.GetTextureScale(p.maskProperty));m.SetTextureOffset("_Emission2ndBlendMask",p.source.GetTextureOffset(p.maskProperty));}
    }
    m.SetFloat("_UseAudioLink",1);m.SetFloat("_AudioLinkAsLocal",1);m.SetFloat("_AudioLinkUVMode",LilMode(p.target.effect));m.SetVector("_AudioLinkUVParams",new Vector4(p.target.spatialScale,p.target.spatialOffset,p.target.uvAngle*Mathf.Deg2Rad,(p.target.audioBand+0.5f)/4f));
    if(p.target.effect==HeartbeatEffectSetup.Effect.SoftBlink)m.SetVector("_AudioLinkUVParams",new Vector4(0,0,0,(p.target.audioBand+0.5f)/4f));
    if(p.target.effect==HeartbeatEffectSetup.Effect.ColorPulse||p.target.effect==HeartbeatEffectSetup.Effect.DoublePulse)m.SetVector("_AudioLinkUVParams",new Vector4(0,0,0,(p.target.audioBand+0.5f)/4f));
    if(p.target.effect==HeartbeatEffectSetup.Effect.RunningLight)m.SetVector("_AudioLinkUVParams",new Vector4(p.target.flowRepeat,0,p.target.flowAxis==0?0:Mathf.PI/2,(p.target.audioBand+0.5f)/4f));
    m.SetVector("_AudioLinkStart",p.target.positionOrigin);
    if(p.target.audioLinkMask!=null){m.SetTexture("_AudioLinkMask",p.target.audioLinkMask);m.SetFloat("_AudioLinkMask_UVMode",p.target.audioLinkMaskUV);}
    if(p.target.effect==HeartbeatEffectSetup.Effect.LilMaskSpectrum)m.SetVector("_AudioLinkUVParams",new Vector4(p.target.spectrumGain,1,1,p.target.spectrumThreshold));m.SetTexture("_AudioLinkLocalMap",p.target.effect==HeartbeatEffectSetup.Effect.SoftBlink?softTex:pulseTex);m.SetVector("_AudioLinkLocalMapParams",new Vector4(0,1,0,0));m.SetFloat("_AudioLink2Emission2nd",1);
    if(p.target.effect==HeartbeatEffectSetup.Effect.DoublePulse)m.SetTexture("_AudioLinkLocalMap",doubleTex);
    if(p.target.effect==HeartbeatEffectSetup.Effect.ColorPulse)m.SetTexture("_AudioLinkLocalMap",Texture2D.whiteTexture);
    m.SetFloat("_AudioLink2Emission",0);m.SetFloat("_AudioLink2Main2nd",0);m.SetFloat("_AudioLink2Main3rd",0);m.SetFloat("_AudioLink2Vertex",0);
    SetupLil(m);
    p.world=Copy(m,output+"/Materials",p.source.name+"_WorldAudioLink");p.world.SetFloat("_AudioLinkAsLocal",0);p.world.SetVector("_AudioLinkDefaultValue",p.target.effect==HeartbeatEffectSetup.Effect.LilMaskSpectrum?new Vector4(0,0,0,10):Vector4.zero);SetupLil(p.world);
    if(p.target.effect==HeartbeatEffectSetup.Effect.LilMaskSpectrum){
     Texture2D mask,table;SpectrumAssets(p.target.audioLinkMask,p.target.spectrumGain,p.target.spectrumThreshold,output,out mask,out table);
     m.SetFloat("_AudioLinkUVMode",3);m.SetTexture("_AudioLinkMask",mask);m.SetTexture("_AudioLinkLocalMap",table);SetupLil(m);
    }
    FloatCurve(lilPulse,p,"_AudioLinkLocalMapParams.z",AnimationCurve.Linear(0,0,60,1));
   } else {
    int ch=p.channel;string cs=ch.ToString();string speed=PoiProperty("_EmissionCenterOutSpeed",ch);string strength=PoiProperty("_EmissionStrength",ch);
    m.SetFloat("_EnableEmission"+cs,1);m.EnableKeyword("POI_EMISSION_"+cs);
    m.SetTexture("_EmissionMask"+cs,p.mask);m.SetFloat("_EmissionMask"+cs+"Channel",0);m.SetFloat("_EmissionMaskInvert"+cs,0);m.SetFloat("_EmissionMask"+cs+"GlobalMask",0);
    if(p.maskProperty!=null){m.SetTextureScale("_EmissionMask"+cs,p.source.GetTextureScale(p.maskProperty));m.SetTextureOffset("_EmissionMask"+cs,p.source.GetTextureOffset(p.maskProperty));}
    m.SetFloat("_EmissionMask"+cs+"UV",p.source.GetFloat("_EmissionMapUV"));
    m.SetColor("_EmissionColor"+cs,p.target.pulseColor);m.SetFloat("_EmissionColor"+cs+"ThemeIndex",0);m.SetFloat("_EmissionBaseColorAsMap"+cs,0);
    m.SetFloat("_EmissionCenterOutEnabled"+cs,IsCenter(p)?1:0);m.SetFloat(speed,0);m.SetFloat(strength,0);
    m.SetTexture("_EmissionMap"+cs,IsCenter(p)?pulseTex:Texture2D.whiteTexture);m.SetTextureScale("_EmissionMap"+cs,Vector2.one);m.SetTextureOffset("_EmissionMap"+cs,Vector2.zero);
    m.SetFloat("_EmissionAL"+cs+"Enabled",0);m.SetOverrideTag(speed+"Animated","1");m.SetOverrideTag(strength+"Animated","1");
    p.world=Copy(m,output+"/Materials",p.source.name+"_WorldAudioLink");
    var w=p.world;w.SetFloat("_EmissionCenterOutEnabled"+cs,0);w.SetTexture("_EmissionMap"+cs,Texture2D.whiteTexture);w.SetFloat("_EnableAudioLink",1);w.EnableKeyword("POI_AUDIOLINK");w.SetFloat("_EmissionAL"+cs+"Enabled",1);
    w.SetVector("_EmissionAL"+cs+"StrengthMod",Vector4.zero);w.SetVector("_EmissionAL"+cs+"Multipliers",Vector4.one);
    w.SetVector("_AudioLinkEmission"+cs+"CenterOut",new Vector4(0,1,0,0));w.SetFloat("_AudioLinkEmission"+cs+"CenterOutBand",0);w.SetFloat("_AudioLinkEmission"+cs+"CenterOutSize",0.1f);w.SetFloat("_AudioLinkEmission"+cs+"CenterOutDuration",0.5f);
    w.SetFloat("_EmissionAL"+cs+"StrengthBand",p.target.audioBand);w.SetFloat("_EmissionAL"+cs+"MultipliersBand",p.target.audioBand);
    w.SetFloat("_AudioLinkEmission"+cs+"CenterOutBand",p.target.audioBand);w.SetFloat("_AudioLinkEmission"+cs+"CenterOutSize",p.target.centerOutThreshold);w.SetFloat("_AudioLinkEmission"+cs+"CenterOutDuration",p.target.centerOutDuration);
    if(!IsCenter(p)){
     w.SetVector("_AudioLinkEmission"+cs+"CenterOut",Vector4.zero);
     float baseline=IsMultiply(p)?p.target.multiplyBase:p.target.addBase;
     if(IsMultiply(p)){w.SetFloat(strength,baseline);w.SetVector("_EmissionAL"+cs+"StrengthMod",Vector4.zero);w.SetVector("_EmissionAL"+cs+"Multipliers",p.target.strengthRange);}
     else{w.SetFloat(strength,baseline);w.SetVector("_EmissionAL"+cs+"StrengthMod",p.target.strengthRange);w.SetVector("_EmissionAL"+cs+"Multipliers",Vector4.one);}
     float min=IsMultiply(p)?baseline*p.target.strengthRange.x:baseline+p.target.strengthRange.x;
     float max=IsMultiply(p)?baseline*p.target.strengthRange.y:baseline+p.target.strengthRange.y;
     FloatCurve(uniform,p,strength,p.target.effect==HeartbeatEffectSetup.Effect.DoublePulse?DoublePulseCurve(min,max):p.target.effect==HeartbeatEffectSetup.Effect.ColorPulse||p.target.effect==HeartbeatEffectSetup.Effect.RunningLight?AnimationCurve.Constant(0,60,1):p.target.effect==HeartbeatEffectSetup.Effect.SoftBlink?SoftBlinkCurve(min,max):new AnimationCurve(new Keyframe(0,min),new Keyframe(20,min),new Keyframe(32,max),new Keyframe(60,min)));Constant(uniformWorld,p,strength,baseline);
    }else{
     w.SetVector("_AudioLinkEmission"+cs+"CenterOut",p.target.strengthRange);
     Constant(low,p,speed,0);Constant(low,p,strength,0);Constant(one,p,speed,1f/3f);Constant(one,p,strength,1);Constant(high,p,speed,85);Constant(high,p,strength,1);Constant(poiWorld,p,speed,0);Constant(poiWorld,p,strength,0);
    }
   }
   if(!p.lil&&(p.target.effect==HeartbeatEffectSetup.Effect.ColorPulse||p.target.effect==HeartbeatEffectSetup.Effect.RunningLight)){p.world.SetFloat("_EmissionStrength"+p.channel,0);p.world.SetVector("_EmissionAL"+p.channel+"StrengthMod",new Vector4(0,1,0,0));Constant(uniformWorld,p,"_EmissionStrength"+p.channel,0);}
   if(p.target.effect==HeartbeatEffectSetup.Effect.ColorPulse){
    string color=p.lil?"_Emission2ndColor":"_EmissionColor"+p.channel;
    ColorCurves(extras,p,color,p.target.restingColor,p.target.pulseColor);
    for(int c=0;c<4;c++)Constant(extrasWorld,p,color+"."+new[]{"r","g","b","a"}[c],p.target.pulseColor[c]);
    p.heart.SetColor(color,p.target.restingColor);p.heart.SetOverrideTag(color+"Animated","1");p.world.SetOverrideTag(color+"Animated","1");
   }
   if(!p.lil&&p.target.effect==HeartbeatEffectSetup.Effect.RunningLight){
    string prop="_EmissionMap"+p.channel;
    p.heart.SetTexture(prop,p.target.flowAxis==0?runHorizontal:runVertical);p.heart.SetTextureScale(prop,Vector2.one*p.target.flowRepeat);
    string offset=prop+"_ST."+(p.target.flowAxis==0?"z":"w");
    FloatCurve(extras,p,offset,AnimationCurve.Linear(0,0,60,1));Constant(extrasWorld,p,offset,0);
    p.heart.SetOverrideTag(prop+"_STAnimated","1");p.world.SetOverrideTag(prop+"_STAnimated","1");
   }
   Swap(heartMode,p,p.heart);Swap(worldMode,p,p.world);
   EditorUtility.SetDirty(p.heart);EditorUtility.SetDirty(p.world);
  }
  var layers=controller.layers;layers[0].name="Heartbeat Material Mode";layers[0].defaultWeight=1;controller.layers=layers;
  var sm=controller.layers[0].stateMachine;var hs=sm.AddState("Heartbeat");hs.motion=heartMode;hs.writeDefaultValues=false;var ws=sm.AddState("World AudioLink");ws.motion=worldMode;ws.writeDefaultValues=false;sm.defaultState=hs;Transition(hs,ws,setup.modeParameter,true);Transition(ws,hs,setup.modeParameter,false);
  if(plans.Exists(p=>p.lil)){
   var settings=AnimationUtility.GetAnimationClipSettings(lilPulse);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(lilPulse,settings);
   controller.AddLayer("lilToon BPM Rim");layers=controller.layers;layers[layers.Length-1].defaultWeight=1;controller.layers=layers;
   var state=controller.layers[layers.Length-1].stateMachine.AddState("BPM Pulse");state.motion=lilPulse;state.speedParameter=setup.bpmParameter;state.speedParameterActive=true;state.writeDefaultValues=false;
  }
  if(plans.Exists(p=>!p.lil&&p.target.effect==HeartbeatEffectSetup.Effect.NativeFlow)){
   controller.AddLayer("Poiyomi BPM Center Out");layers=controller.layers;layers[layers.Length-1].defaultWeight=1;controller.layers=layers;sm=controller.layers[layers.Length-1].stateMachine;
   var tree=new BlendTree{name="BPM to Flow Speed",blendType=BlendTreeType.Simple1D,blendParameter=setup.bpmParameter,useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(tree,controller);tree.AddChild(low,0);tree.AddChild(one,1);tree.AddChild(high,255);
   hs=sm.AddState("Heartbeat Center Out");hs.motion=tree;hs.writeDefaultValues=false;ws=sm.AddState("World AudioLink");ws.motion=poiWorld;ws.writeDefaultValues=false;sm.defaultState=hs;Transition(hs,ws,setup.modeParameter,true);Transition(ws,hs,setup.modeParameter,false);
  }
  if(plans.Exists(p=>!p.lil&&!IsCenter(p))){
   var settings=AnimationUtility.GetAnimationClipSettings(uniform);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(uniform,settings);
   controller.AddLayer("Poiyomi Uniform Pulse");layers=controller.layers;layers[layers.Length-1].defaultWeight=1;controller.layers=layers;sm=controller.layers[layers.Length-1].stateMachine;
   hs=sm.AddState("Heartbeat Uniform");hs.motion=uniform;hs.speedParameter=setup.bpmParameter;hs.speedParameterActive=true;hs.writeDefaultValues=false;ws=sm.AddState("World AudioLink");ws.motion=uniformWorld;ws.writeDefaultValues=false;sm.defaultState=hs;Transition(hs,ws,setup.modeParameter,true);Transition(ws,hs,setup.modeParameter,false);
  }
  if(AnimationUtility.GetCurveBindings(extras).Length>0){
   var settings=AnimationUtility.GetAnimationClipSettings(extras);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(extras,settings);
   controller.AddLayer("Pulselink Color and Flow");layers=controller.layers;layers[layers.Length-1].defaultWeight=1;controller.layers=layers;sm=controller.layers[layers.Length-1].stateMachine;
   hs=sm.AddState("Heartbeat");hs.motion=extras;hs.speedParameter=setup.bpmParameter;hs.speedParameterActive=true;hs.writeDefaultValues=false;
   ws=sm.AddState("World AudioLink");ws.motion=extrasWorld;ws.writeDefaultValues=false;sm.defaultState=hs;Transition(hs,ws,setup.modeParameter,true);Transition(ws,hs,setup.modeParameter,false);
  }
  var menu=ScriptableObject.CreateInstance<VRCExpressionsMenu>();menu.name="Heartbeat Menu";menu.controls.Add(new VRCExpressionsMenu.Control{name=setup.menuLabel,type=VRCExpressionsMenu.Control.ControlType.Toggle,parameter=new VRCExpressionsMenu.Control.Parameter{name=setup.modeParameter},value=1});AssetDatabase.CreateAsset(menu,output+"/Menu.asset");
  foreach(var asset in new UnityEngine.Object[]{controller,extras,extrasWorld,heartMode,worldMode,lilPulse,low,one,high,poiWorld,uniform,uniformWorld,menu}){EditorUtility.SetDirty(asset);AssetDatabase.SaveAssetIfDirty(asset);}
  foreach(var p in plans){AssetDatabase.SaveAssetIfDirty(p.heart);AssetDatabase.SaveAssetIfDirty(p.world);}
  Undo.RecordObject(setup,"Configure Heartbeat");
  var merge=setup.GetComponent<ModularAvatarMergeAnimator>();if(merge==null)merge=Undo.AddComponent<ModularAvatarMergeAnimator>(setup.gameObject);Undo.RecordObject(merge,"Set Heartbeat Animator");merge.animator=controller;merge.layerType=VRCAvatarDescriptor.AnimLayerType.FX;merge.pathMode=MergeAnimatorPathMode.Absolute;merge.matchAvatarWriteDefaults=true;
  var parameters=setup.GetComponent<ModularAvatarParameters>();if(parameters==null)parameters=Undo.AddComponent<ModularAvatarParameters>(setup.gameObject);Undo.RecordObject(parameters,"Set Heartbeat Parameters");
  parameters.parameters=new List<ParameterConfig>{
   new ParameterConfig{nameOrPrefix=setup.bpmParameter,syncType=ParameterSyncType.Int,saved=false,localOnly=false,internalParameter=false},
   new ParameterConfig{nameOrPrefix=setup.modeParameter,syncType=ParameterSyncType.Bool,saved=true,localOnly=false,internalParameter=false,hasExplicitDefaultValue=true,defaultValue=0}};
  var installer=setup.GetComponent<ModularAvatarMenuInstaller>();if(installer==null)installer=Undo.AddComponent<ModularAvatarMenuInstaller>(setup.gameObject);Undo.RecordObject(installer,"Set Heartbeat Menu");installer.menuToAppend=menu;
  foreach(var p in plans){Undo.RecordObject(p.target.renderer,"Assign Heartbeat Material");var materials=p.target.renderer.sharedMaterials;if(p.target.restoreMaterial==null)p.target.restoreMaterial=materials[p.target.materialSlot];materials[p.target.materialSlot]=p.heart;p.target.renderer.sharedMaterials=materials;p.target.sourceMaterial=p.source;if(p.target.restoreMaterial==null)p.target.restoreMaterial=p.source;p.target.generatedHeartbeat=p.heart;p.target.generatedAudioLink=p.world;PrefabUtility.RecordPrefabInstancePropertyModifications(p.target.renderer);}
  setup.generatedFolder=output;setup.lastBuildSummary=plans.Count+" Material-Slots eingerichtet. Aus: Herzpuls. An: Welt-AudioLink.";
  foreach(var component in new Component[]{setup,merge,parameters,installer}){EditorUtility.SetDirty(component);PrefabUtility.RecordPrefabInstancePropertyModifications(component);}
  UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(setup.gameObject.scene);
 }
}
}
