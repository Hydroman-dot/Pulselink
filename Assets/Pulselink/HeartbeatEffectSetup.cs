using System;
using System.Collections.Generic;
using UnityEngine;
namespace HeartbeatEmission {
[DisallowMultipleComponent]
[AddComponentMenu("Pulselink/Setup")]
public class HeartbeatEffectSetup : MonoBehaviour, VRC.SDKBase.IEditorOnly {
 public enum Effect { NativeFlow=0, UniformPulse=1, LilUV=2, LilMask=3, LilMaskSpectrum=4, LilPosition=5, PoiStrengthMultiply=6, PoiStrengthAdd=7, SoftBlink=8, RunningLight=9, ColorPulse=10, DoublePulse=11 }
 [Serializable] public class Target {
  public Effect effect=Effect.NativeFlow;
  public Renderer renderer;
  [Min(0)] public int materialSlot;
  public Material sourceMaterial;
  [HideInInspector] public Material restoreMaterial;
  public Texture emissionMask;
  [ColorUsage(true,true)] public Color pulseColor=new Color(2.5f,0.025f,0.04f,1);
  [ColorUsage(true,true)] public Color restingColor=new Color(0.02f,0,0,1);
  public int flowAxis;
  [Min(0.1f)] public float flowRepeat=1;
  public Texture audioLinkMask;
  public int audioLinkMaskUV;
  public int audioBand;
  public float spatialScale=0.65f;
  public float spatialOffset;
  public float uvAngle;
  public Vector3 positionOrigin;
  public float spectrumGain=1;
  public float spectrumThreshold=0.1f;
  public Vector2 strengthRange=new Vector2(0,1);
  public float multiplyBase=1;
  public float addBase;
  public float centerOutDuration=0.5f;
  public float centerOutThreshold=0.1f;
  [HideInInspector] public Material generatedHeartbeat;
  [HideInInspector] public Material generatedAudioLink;
 }
 public string bpmParameter="VRCOSC/Heartrate/Value";
 public string modeParameter="HBE/WorldAudioLink";
 public string menuLabel="Welt-AudioLink";
 public List<Target> targets=new List<Target>();
 [HideInInspector] public string generatedFolder;
 [HideInInspector] public string lastBuildSummary;
}
}