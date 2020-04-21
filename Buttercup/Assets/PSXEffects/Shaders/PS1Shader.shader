// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

Shader "PSXEffects/PS1Shader"
{
	Properties
	{
		[Toggle] _Unlit("Unlit", Float) = 0.0
		[Toggle] _DrawDist("Affected by Polygonal Draw Distance", Float) = 1.0
		_VertexInaccuracy("Vertex Inaccuracy Override", Float) = -1.0
		_Color("Color", Color) = (1,1,1,1)
		[KeywordEnum(Vertex, Fragment)] _DiffModel("Diffuse Model", Float) = 0.0
		_MainTex("Texture", 2D) = "white" {}
		_LODTex("LOD Texture", 2D) = "white" {}
		_LODAmt("LOD Amount", Float) = 0.0
		_NormalMap("Normal Map", 2D) = "bump" {}
		[KeywordEnum(Gouraud, Phong)] _SpecModel("Specular Model", Float) = 0.0
		_SpecularMap("Specular Map", 2D) = "white" {}
		_Specular("Specular Amount", Float) = 0.0
		_MetalMap("Metal Map", 2D) = "white" {}
		_Metallic("Metallic Amount", Range(0.0,1.0)) = 0.0
		_Smoothness("Smoothness Amount", Range(0.0,1.0)) = 0.5
		_Emission("Emission Map", 2D) = "white" {}
		_EmissionAmt("Emission Amount", Float) = 0.0
		_Cube("Cubemap", Cube) = "" {}

		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _Cul("__cul", Float) = 0.0

		[HideInInspector] _RenderMode("__rnd", Float) = 0.0
	}

		SubShader
		{
			Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
			LOD 100
			Lighting On
			Offset[_Offset], 1
			Cull[_Cul]
			Blend[_SrcBlend][_DstBlend]

			Pass
			{
				Tags { "LightMode" = "ForwardBase" }
				CGPROGRAM

				

				float _Transparent;

				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc"
				#include "AutoLight.cginc"
				#include "PSXEffects.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap
				#pragma multi_compile_fog
				#pragma multi_compile _ LIGHTMAP_ON
				#pragma shader_feature TRANSPARENT
				#pragma shader_feature BFC

				struct appdata {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float4 texcoord : TEXCOORD0;
					float4 color : COLOR;
					float3 tangent: TANGENT;
					float4 texcoord1 : TEXCOORD1;
				};

				struct v2f
				{
					float4 uv : TEXCOORD0;
					fixed4 color : COLOR;
					fixed4 diff : COLOR1;
					fixed3 spec : COLOR2;
					float4 pos : SV_POSITION;
					float4 vertPos : TEXCOORD1;
					float3 normal : NORMAL;
					float3 normalDir : TEXCOORD2;
					float3 viewDir : TEXCOORD3;
					float3 lightDir : TEXCOORD4;

					float3 T : TEXCOORD5;
					float3 B : TEXCOORD6;
					float3 N : TEXCOORD7;
					LIGHTING_COORDS(8, 9)
					UNITY_FOG_COORDS(10)
					float4 uv1 : TEXCOORD11;
				};

				v2f vert(appdata v)
				{
					v2f o;

					// Vertex inaccuracy block
					float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
					float3 viewDir = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
					worldPos.xyz += _WorldSpaceCameraPos.xyz * _CamPos;
					worldPos.xyz += viewDir * 100 * _CamPos;
					o.pos = UnityObjectToClipPos(v.vertex);
					if (_VertexInaccuracy < 0) _VertexInaccuracy = _VertexSnappingDetail;
					if (_WorldSpace == 1) {
						_VertexInaccuracy /= 2048;
						worldPos.xyz /= _VertexInaccuracy;
						worldPos.xyz = round(worldPos.xyz);
						worldPos.xyz *= _VertexInaccuracy;
						worldPos.xyz -= _WorldSpaceCameraPos.xyz * _CamPos;
						worldPos.xyz -= viewDir * 100 * _CamPos;
						v.vertex = mul(unity_WorldToObject, worldPos);
						o.pos = UnityObjectToClipPos(v.vertex);
					}
					else {
						o.pos = UnityObjectToClipPos(v.vertex);
						o.pos = PixelSnap(o.pos);
					}

					// Set UV outputs
					float wVal = mul(UNITY_MATRIX_P, o.pos).z;
					if(_AffineMapping)
						o.uv = float4(v.texcoord.xy * wVal, wVal, 0);
					else
						o.uv = float4(v.texcoord.xyz, 0);

					// Currently no difference from non-affine mapping
					#ifdef LIGHTMAP_ON
					if (_AffineMapping)
						o.uv1 = float4(v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw, wVal, 0);
					else
						o.uv1 = float4(v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw, 0, 0);
					#endif

					

					float3 worldNormal = UnityObjectToWorldNormal(v.normal);
					
					// Set cutoff value for vertex render distance
					o.diff.a = (distance(worldPos, _WorldSpaceCameraPos) > _DrawDistance && _DrawDistance > 0);
					// Set value for LOD distance
					o.uv.a = (distance(worldPos, _WorldSpaceCameraPos) > _LODAmt && _LODAmt > 0);

					// Various outputs needed for fragment
					o.color = v.color;
					o.normal = v.normal;
					o.vertPos = v.vertex;

					o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
					o.normalDir = normalize(mul(v.normal, unity_WorldToObject).xyz);

					// Gouraud (per-vertex) specular model
					float3 lightDir;
					if (_WorldSpaceLightPos0.w == 0.0) {
						lightDir = normalize(_WorldSpaceLightPos0.xyz);
					}
					else {
						float3 vertToLight = _WorldSpaceLightPos0.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
						float dist = length(vertToLight);
						lightDir = normalize(vertToLight);
					}

					o.spec = float3(0.0, 0.0, 0.0);
					if (dot(o.normalDir, lightDir) >= 0.0 || !_SpecModel && 1.0) {
						float3 reflection = reflect(lightDir, worldNormal);
						float3 viewDir = normalize(o.viewDir);
						o.spec = saturate(dot(reflection, -o.viewDir));
						o.spec = pow(o.spec, 20.0f);
					}

					// Calculate vertex lighting
					o.diff.rgb = float3(0, 0, 0);
					if (_DiffModel == 0) {
						float nl = (max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz)));
						o.diff.rgb = nl * _LightColor0;
						o.diff.rgb += ShadeSH9(half4(worldNormal, 1));
					}


					#ifdef VERTEXLIGHT_ON
					for (int j = 0; j < 4; j++) {
						float4 lightPos = float4(unity_4LightPosX0[j], unity_4LightPosY0[j], unity_4LightPosZ0[j], 1.0);

						float3 vertexToLightSource = lightPos.xyz - worldPos.xyz;
						float3 lightDir = normalize(vertexToLightSource);
						float squaredDist = dot(vertexToLightSource, vertexToLightSource);
						float atten = 1.0 / (1.0 + unity_4LightAtten0[j] * squaredDist);
						o.diff.rgb += atten * unity_LightColor[j].rgb * _Color.rgb * max(0.0, dot(o.normal, lightDir));
					}
					#endif

					

					o.lightDir = lightDir;

					// Outputs needed for calculating world normal in fragment
					worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
					float3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.normal);

					float3 binormal = cross(v.normal, v.tangent.xyz);
					float3 worldBinormal = mul((float3x3)unity_ObjectToWorld, -binormal);

					o.N = normalize(worldNormal);
					o.T = normalize(worldTangent);
					o.B = normalize(worldBinormal);

					UNITY_TRANSFER_FOG(o, o.pos);
					TRANSFER_VERTEX_TO_FRAGMENT(o);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{

					float2 adjUv = PerformAffineMapping(i.uv, _MainTex_ST, _AffineMapping);
					float2 adjUV1 = PerformAffineMapping(i.uv1, unity_LightmapST, _AffineMapping);
					float4 albedo = tex2D(_MainTex, adjUv);
					// Lerp between main texture and LOD texture depending on LOD distance
					float4 lod = tex2D(_LODTex, adjUv);
					if (i.uv.a && lod.r + lod.g + lod.b < 3.0)
						albedo = lod;
					float4 col = float4(1,1,1,1);

					if (!_Unlit) {
						// Normals
						float3 tangentNormal = tex2D(_NormalMap, adjUv).xyz;
						tangentNormal = normalize(tangentNormal * 2 - 1);
						float3x3 TBN = float3x3(normalize(i.T), normalize(i.B) * 8, normalize(i.N));
						TBN = transpose(TBN);
						float3 worldNormal = mul(TBN, tangentNormal);

						// Calculate metal/smoothness map
						float3 reflectedDir = reflect(i.viewDir, normalize(i.normalDir));
						float4 metalMap = tex2D(_MetalMap, adjUv);
						UnityIndirect indirectLight;
						indirectLight.diffuse = max(0, ShadeSH9(half4(i.normal, 1)));
						indirectLight.specular = 0;
						_Smoothness *= metalMap.a;
						float roughness = 1 - _Smoothness;
						float3 reflectionDir = reflect(-i.viewDir, i.normal);
						float4 envRefl = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectionDir, roughness * 6);
						indirectLight.specular = DecodeHDR(envRefl, unity_SpecCube0_HDR);

						// Apply lighting
						float3 lightDir = normalize(i.lightDir);
						float4 diffuse = float4(1, 1, 1, albedo.a);
						#if !defined(LIGHTMAP_ON)
						if (_DiffModel == 1) {
							float nl = (max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz)));
							diffuse.rgb = nl * _LightColor0;
							diffuse.rgb += ShadeSH9(half4(worldNormal, 1));
							diffuse.rgb += i.diff.rgb;
						} else {
							diffuse.rgb = i.diff.rgb;
						}
						#endif
						diffuse.rgb *= albedo.rgb;

						// Phong specular model
						float3 specular = i.spec;
						if (diffuse.r > 0 && _SpecModel) {
							if (_WorldSpaceLightPos0.w == 0.0) {
								lightDir = normalize(_WorldSpaceLightPos0.xyz);
							}
							else {
								float3 vertToLight = _WorldSpaceLightPos0.xyz - mul(unity_ObjectToWorld, i.pos).xyz;
								lightDir = normalize(vertToLight);
							}

							float3 reflection = reflect(lightDir, worldNormal);
							float3 viewDir = normalize(i.viewDir);
							specular = pow(saturate(dot(reflection, -viewDir)), 20.0f);
						}
						float4 specularIntensity = tex2D(_SpecularMap, adjUv) * _Specular;
						specular *= specularIntensity;

						// Apply lighting calculations from above
						col.rgb = diffuse.rgb;
						col.rgb *= (indirectLight.diffuse + indirectLight.specular) * _Metallic * metalMap.r;
						col.rgb += diffuse * (1 - _Metallic) + specular;
						#ifdef LIGHTMAP_ON
						col.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv1)) + 0.6 * UNITY_LIGHTMODEL_AMBIENT;
						#endif
						// Add cubemap to output color
						col.rgb += texCUBE(_Cube, reflectedDir) / 2 - 0.25;
						// Tint material
						col *= i.color * _Color;
						col.a = albedo.a * i.color.a * _Color.a;
						// Darken darks
						col.rgb -= max(0, (1 - diffuse.rgb) * i.color) * _DarkMax;
						// Emission map
						col.rgb += tex2D(_Emission, adjUv) * _EmissionAmt;
						// Lighting/shadows
						col.rgb *= LIGHT_ATTENUATION(i);
					} else {
						// If material is unlit, just set color to albedo
						col.rgb += albedo;
						// Tint material
						col *= i.color * _Color;
						col.a = albedo.a * i.color.a * _Color.a;
					}

					if (i.diff.a && _DrawDist == 1.0 || (_RenderMode == 2.0 && albedo.a == 0)) {
						// Don't draw if outside render distance
						clip(-1);
					}

					UNITY_APPLY_FOG(i.fogCoord, col.rgb);

					return col;
				}
				ENDCG
			}

			// Pass for realtime shadows
			Pass
			{
				Tags{ "LightMode" = "ShadowCaster" }

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#include "UnityCG.cginc"
				#include "PSXEffects.cginc"

				struct appdata {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float4 texcoord : TEXCOORD0;
					float4 color : COLOR;
					float3 tangent: TANGENT;
				};

				struct v2f {
					V2F_SHADOW_CASTER;
					float3 uv : TEXCOORD1;
				};
				struct v2f_fragment {
					UNITY_VPOS_TYPE vpos : VPOS;
					float3 uv : TEXCOORD1;
				};

				v2f vert(appdata v)
				{
					v2f o;

					// Vertex inaccuracy block
					float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
					float3 viewDir = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
					worldPos.xyz += _WorldSpaceCameraPos.xyz * _CamPos;
					worldPos.xyz += viewDir * 100 * _CamPos;
					o.pos = UnityObjectToClipPos(v.vertex);
					if (_VertexInaccuracy < 0) _VertexInaccuracy = _VertexSnappingDetail;
					if (_WorldSpace == 1) {
						_VertexInaccuracy /= 2048;
						worldPos.xyz /= _VertexInaccuracy;
						worldPos.xyz = round(worldPos.xyz);
						worldPos.xyz *= _VertexInaccuracy;
						worldPos.xyz -= _WorldSpaceCameraPos.xyz * _CamPos;
						worldPos.xyz -= viewDir * 100 * _CamPos;
						v.vertex = mul(unity_WorldToObject, worldPos);
						o.pos = UnityObjectToClipPos(v.vertex);
					}
					else {
						o.pos = UnityObjectToClipPos(v.vertex);
						o.pos = PixelSnap(o.pos);
					}

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
					float wVal = mul(UNITY_MATRIX_P, v.vertex).z;
					if(_AffineMapping)
						o.uv = float3(v.texcoord.xy * wVal, wVal);
					else
						o.uv = v.texcoord;
					
					return o;
				}

				fixed _Cutoff;
				float4 frag(v2f_fragment i) : SV_Target
				{
					if (_RenderMode == 1 || _RenderMode == 2) {
						float2 adjUv = i.uv.xy;
						if (_AffineMapping)
							adjUv = (i.uv / i.uv.z + _MainTex_ST.zw) * _MainTex_ST.xy;
						else
							adjUv = (i.uv + _MainTex_ST.zw) * _MainTex_ST.xy;

						fixed4 texcol = tex2D(_MainTex, adjUv);
						clip(GetDither(i.vpos, _Color.a) * texcol.a * _Color.a - _Cutoff);
					}

					SHADOW_CASTER_FRAGMENT(i);
				}
				ENDCG
			}

			// Pass for extra lights
			// Most of the code here is from the first pass
			Pass
			{
				Tags { "LightMode" = "ForwardAdd" }
				Blend One One
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdadd_fullshadows

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "PSXEffects.cginc"

				struct v2f
				{
					float4 pos : SV_POSITION;
					float3 uv : TEXCOORD0;
					float3 lightDir : TEXCOORD1;
					float3 normal : TEXCOORD4;
					fixed4 diff : COLOR;
					LIGHTING_COORDS(2,3)
					float2 uv1 : TEXCOORD11;
				};

				v2f vert(appdata_tan v) {
					v2f o;

					float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
					float3 viewDir = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
					worldPos.xyz += _WorldSpaceCameraPos.xyz * _CamPos;
					worldPos.xyz += viewDir * 100 * _CamPos;
					if (_VertexInaccuracy < 0) _VertexInaccuracy = _VertexSnappingDetail;
					if (_WorldSpace == 1) {
						_VertexInaccuracy /= 2048;
						worldPos.xyz /= _VertexInaccuracy;
						worldPos.xyz = round(worldPos.xyz);
						worldPos.xyz *= _VertexInaccuracy;
						worldPos.xyz -= _WorldSpaceCameraPos.xyz * _CamPos;
						worldPos.xyz -= viewDir * 100 * _CamPos;
						v.vertex = mul(unity_WorldToObject, worldPos);
						o.pos = UnityObjectToClipPos(v.vertex);
					}
					else {
						o.pos = UnityObjectToClipPos(v.vertex);
						o.pos = PixelSnap(o.pos);
					}

					o.diff.a = 1;

					if (distance(worldPos, _WorldSpaceCameraPos) > _DrawDistance && _DrawDistance > 0) {
						o.diff.a = 0;
					}

					float wVal = mul(UNITY_MATRIX_P, o.pos).z;
					if(_AffineMapping)
						o.uv = float3(v.texcoord.xy * wVal, wVal);
					else
						o.uv = v.texcoord;

					#ifdef LIGHTMAP_ON
					o.uv1 = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#endif

					o.lightDir = ObjSpaceLightDir(v.vertex);

					o.normal = v.normal;
					TRANSFER_VERTEX_TO_FRAGMENT(o);
					return o;
				}

				fixed4 _LightColor0;

				fixed4 frag(v2f i) : COLOR
				{
					float2 adjUv = i.uv.xy;
					if (_AffineMapping)
						adjUv = (i.uv / i.uv.z + _MainTex_ST.zw) * _MainTex_ST.xy;
					else
						adjUv = (i.uv + _MainTex_ST.zw) * _MainTex_ST.xy;
					fixed4 tex = tex2D(_MainTex, adjUv);
					if (i.diff.a == 0 && _DrawDist == 1.0 || (_RenderMode == 2.0 && tex.a == 0)) {
						clip(-1);
					}

					i.lightDir = normalize(i.lightDir);

					fixed atten = LIGHT_ATTENUATION(i);
					if (0.0 == _WorldSpaceLightPos0.w) {
						atten = 1.0;
						i.lightDir = normalize(_WorldSpaceLightPos0.xyz);
					}

					tex *= _Color;

					fixed diff = saturate(dot(normalize(i.normal), i.lightDir));

					fixed4 col;
					col.rgb = (tex.rgb * _LightColor0.rgb * diff) * (atten * 2);
					#ifdef LIGHTMAP_ON
					col.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv1));
					#endif
					col.a = tex.a;
					return col;
				}

				ENDCG
			}
		}
		CustomEditor "PS1ShaderEditor"
		Fallback "VertexLit"
}
