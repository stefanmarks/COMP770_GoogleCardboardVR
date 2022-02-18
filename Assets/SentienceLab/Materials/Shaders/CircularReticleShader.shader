﻿// Copyright 2015 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "UI/Circular Reticle" {
    Properties{
        _Color("Color", Color) = (1, 1, 1, 1)
        _ColorFuse("Fuse Color", Color) = (1, 0, 0, 1)
        _FuseTime("Fuse Time", Range(0.0, 1.0)) = 0
        _InnerDiameter ("Inner Diameter", Range(0, 10.0)) = 1.5
        _OuterDiameter ("Outer Diameter", Range(0.00872665, 10.0)) = 2.0
        _DistanceInMeters ("Distance in Meters", Range(0.0, 100.0)) = 2.0
    }

  SubShader {
    Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Pass {
      Blend SrcAlpha OneMinusSrcAlpha, OneMinusDstAlpha One
      AlphaTest Off
      Cull Back
      Lighting Off
      ZWrite Off
      ZTest Always

      Fog { Mode Off }
      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      uniform float4 _Color;
      uniform float4 _ColorFuse;
      uniform float  _FuseTime;
      uniform float  _InnerDiameter;
      uniform float  _OuterDiameter;
      uniform float  _DistanceInMeters;

      struct vertexInput {
        float4 vertex : POSITION;
      };

      struct fragmentInput{
          float4 position : SV_POSITION;
          float2 uv       : TEXCOORD;
      };

      fragmentInput vert(vertexInput i) {
        float scale = lerp(_OuterDiameter, _InnerDiameter, i.vertex.z);

        float3 vert_out = float3(i.vertex.x * scale, i.vertex.y * scale, _DistanceInMeters);

        fragmentInput o;
        o.position = UnityObjectToClipPos(vert_out);
        o.uv       = i.vertex.xy;
        return o;
      }

      fixed4 frag(fragmentInput i) : SV_Target{
        float f = step((atan2(-i.uv.x, -i.uv.y) / (2*3.14159274)) + 0.50001, _FuseTime);
        return lerp(_Color, _ColorFuse, f);
      }

      ENDCG
    }
  }
}
