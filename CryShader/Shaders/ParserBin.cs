using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using FXMacroBin = System.Collections.Generic.Dictionary<System.UInt32, CryShader.Shaders.SMacroBinFX>;
using ShaderTokensVec = System.Collections.Generic.List<System.UInt32>;
using FXShaderToken = System.Collections.Generic.List<CryShader.Shaders.STokenD>;
using CryShader.Core;
using System.Text;

namespace CryShader.Shaders
{
    public partial class CParserBin
    {
        internal static string[] g_KeyTokens = new string[(int)EToken.eT_max];
        internal static List<bool> sfxIFDef = new List<bool>();
        internal static List<bool> sfxIFIgnore = new List<bool>();
    }

    // key tokens
    public enum EToken
    {
        eT_unknown = 0,
        eT_include = 1,
        eT_define = 2,
        eT_define_2 = 3,
        eT_undefine = 4,

        eT_fetchinst = 5,
        eT_if = 6,
        eT_ifdef = 7,
        eT_ifndef = 8,
        eT_if_2 = 9,
        eT_ifdef_2 = 10,
        eT_ifndef_2 = 11,
        eT_elif = 12,
        eT_elif_2 = 13,

        eT_endif = 14,
        eT_else = 15,
        eT_or = 16,
        eT_and = 17,
        eT_warning = 18,
        eT_register_env = 19,
        eT_ifcvar = 20,
        eT_ifncvar = 21,
        eT_elifcvar = 22,
        eT_skip = 23,
        eT_skip_1 = 24,
        eT_skip_2 = 25,

        eT_br_rnd_1 = 26,
        eT_br_rnd_2 = 27,
        eT_br_sq_1 = 28,
        eT_br_sq_2 = 29,
        eT_br_cv_1 = 30,
        eT_br_cv_2 = 31,
        eT_br_tr_1 = 32,
        eT_br_tr_2 = 33,
        eT_comma = 34,
        eT_dot = 35,
        eT_colon = 36,
        eT_semicolumn = 37,
        eT_excl = 38, // !
        eT_quote = 39,
        eT_sing_quote = 40,

        eT_question = 41,
        eT_eq = 42,
        eT_plus = 43,
        eT_minus = 44,
        eT_div = 45,
        eT_mul = 46,
        eT_dot_math = 47,
        eT_mul_math = 48,
        eT_sqrt_math = 49,
        eT_exp_math = 50,
        eT_log_math = 51,
        eT_log2_math = 52,
        eT_sin_math = 53,
        eT_cos_math = 54,
        eT_sincos_math = 55,
        eT_floor_math = 56,
        eT_ceil_math = 57,
        eT_frac_math = 58,
        eT_lerp_math = 59,
        eT_abs_math = 60,
        eT_clamp_math = 61,
        eT_min_math = 62,
        eT_max_math = 63,
        eT_length_math = 64,

        eT_tex2D,
        eT_tex2Dproj,
        eT_tex3D,
        eT_texCUBE,
        eT_SamplerState,
        eT_SamplerComparisonState,
        eT_sampler_state,
        eT_Texture2D,
        eT_RWTexture2D,
        eT_RWTexture2DArray,
        eT_Texture2DArray,
        eT_Texture2DMS,
        eT_TextureCube,
        eT_TextureCubeArray,
        eT_Texture3D,
        eT_RWTexture3D,

        eT_snorm,
        eT_unorm,
        eT_float,
        eT_float2,
        eT_float3,
        eT_float4,
        eT_float2x4,
        eT_float3x4,
        eT_float4x4,
        eT_float3x3,
        eT_half,
        eT_half2,
        eT_half3,
        eT_half4,
        eT_half2x4,
        eT_half3x4,
        eT_half4x4,
        eT_half3x3,
        eT_bool,
        eT_int,
        eT_int2,
        eT_int3,
        eT_int4,
        eT_uint,
        eT_uint2,
        eT_uint3,
        eT_uint4,
        eT_min16float,
        eT_min16float2,
        eT_min16float3,
        eT_min16float4,
        eT_min16float4x4,
        eT_min16float3x4,
        eT_min16float2x4,
        eT_min16float3x3,
        eT_min10float,
        eT_min10float2,
        eT_min10float3,
        eT_min10float4,
        eT_min10float4x4,
        eT_min10float3x4,
        eT_min10float2x4,
        eT_min10float3x3,
        eT_min16int,
        eT_min16int2,
        eT_min16int3,
        eT_min16int4,
        eT_min12int,
        eT_min12int2,
        eT_min12int3,
        eT_min12int4,
        eT_min16uint,
        eT_min16uint2,
        eT_min16uint3,
        eT_min16uint4,

        eT_sampler1D,
        eT_sampler2D,
        eT_sampler3D,
        eT_samplerCUBE,
        eT_const,

        eT_inout,

        eT_struct,
        eT_sampler,
        eT_TEXCOORDN,
        eT_TEXCOORD0,
        eT_TEXCOORD1,
        eT_TEXCOORD2,
        eT_TEXCOORD3,
        eT_TEXCOORD4,
        eT_TEXCOORD5,
        eT_TEXCOORD6,
        eT_TEXCOORD7,
        eT_TEXCOORD8,
        eT_TEXCOORD9,
        eT_TEXCOORD10,
        eT_TEXCOORD11,
        eT_TEXCOORD12,
        eT_TEXCOORD13,
        eT_TEXCOORD14,
        eT_TEXCOORD15,
        eT_TEXCOORD16,
        eT_TEXCOORD17,
        eT_TEXCOORD18,
        eT_TEXCOORD19,
        eT_TEXCOORD20,
        eT_TEXCOORD21,
        eT_TEXCOORD22,
        eT_TEXCOORD23,
        eT_TEXCOORD24,
        eT_TEXCOORD25,
        eT_TEXCOORD26,
        eT_TEXCOORD27,
        eT_TEXCOORD28,
        eT_TEXCOORD29,
        eT_TEXCOORD30,
        eT_TEXCOORD31,
        eT_TEXCOORDN_centroid,
        eT_TEXCOORD0_centroid,
        eT_TEXCOORD1_centroid,
        eT_TEXCOORD2_centroid,
        eT_TEXCOORD3_centroid,
        eT_TEXCOORD4_centroid,
        eT_TEXCOORD5_centroid,
        eT_TEXCOORD6_centroid,
        eT_TEXCOORD7_centroid,
        eT_TEXCOORD8_centroid,
        eT_TEXCOORD9_centroid,
        eT_TEXCOORD10_centroid,
        eT_TEXCOORD11_centroid,
        eT_TEXCOORD12_centroid,
        eT_TEXCOORD13_centroid,
        eT_TEXCOORD14_centroid,
        eT_TEXCOORD15_centroid,
        eT_TEXCOORD16_centroid,
        eT_TEXCOORD17_centroid,
        eT_TEXCOORD18_centroid,
        eT_TEXCOORD19_centroid,
        eT_TEXCOORD20_centroid,
        eT_TEXCOORD21_centroid,
        eT_TEXCOORD22_centroid,
        eT_TEXCOORD23_centroid,
        eT_TEXCOORD24_centroid,
        eT_TEXCOORD25_centroid,
        eT_TEXCOORD26_centroid,
        eT_TEXCOORD27_centroid,
        eT_TEXCOORD28_centroid,
        eT_TEXCOORD29_centroid,
        eT_TEXCOORD30_centroid,
        eT_TEXCOORD31_centroid,
        eT_COLOR0,
        eT_static,
        eT_shared,
        eT_groupshared,
        eT_packoffset,
        eT_register,
        eT_return,
        eT_vsregister,
        eT_psregister,
        eT_gsregister,
        eT_dsregister,
        eT_hsregister,
        eT_csregister,

        eT_slot,
        eT_vsslot,
        eT_psslot,
        eT_gsslot,
        eT_dsslot,
        eT_hsslot,
        eT_csslot,

        eT_StructuredBuffer,
        eT_RWStructuredBuffer,
        eT_ByteAddressBuffer,
        eT_RWByteAddressBuffer,
        eT_Buffer,
        eT_RWBuffer,

        eT_color,
        eT_Position,
        eT_Allways,

        eT_STANDARDSGLOBAL,

        eT_technique,
        eT_string,
        eT_UIName,
        eT_UIDescription,
        eT_UIWidget,
        eT_UIWidget0,
        eT_UIWidget1,
        eT_UIWidget2,
        eT_UIWidget3,

        eT_Texture,
        eT_Filter,
        eT_MinFilter,
        eT_MagFilter,
        eT_MipFilter,
        eT_AddressU,
        eT_AddressV,
        eT_AddressW,
        eT_BorderColor,
        eT_sRGBLookup,

        eT_LINEAR,
        eT_POINT,
        eT_NONE,
        eT_ANISOTROPIC,
        eT_MIN_MAG_MIP_POINT,
        eT_MIN_MAG_MIP_LINEAR,
        eT_MIN_MAG_LINEAR_MIP_POINT,
        eT_COMPARISON_MIN_MAG_LINEAR_MIP_POINT,
        eT_MINIMUM_MIN_MAG_MIP_LINEAR,
        eT_MAXIMUM_MIN_MAG_MIP_LINEAR,

        eT_Clamp,
        eT_Border,
        eT_Wrap,
        eT_Mirror,

        eT_Script,
        eT_comment,
        eT_asm,

        eT_RenderOrder,
        eT_ProcessOrder,
        eT_RenderCamera,
        eT_RenderType,
        eT_RenderFilter,
        eT_RenderColorTarget1,
        eT_RenderDepthStencilTarget,
        eT_ClearSetColor,
        eT_ClearSetDepth,
        eT_ClearTarget,
        eT_RenderTarget_IDPool,
        eT_RenderTarget_UpdateType,
        eT_RenderTarget_Width,
        eT_RenderTarget_Height,
        eT_GenerateMips,

        eT_PreProcess,
        eT_PostProcess,
        eT_PreDraw,

        eT_WaterReflection,
        eT_Panorama,

        eT_WaterPlaneReflected,
        eT_PlaneReflected,
        eT_Current,

        eT_CurObject,
        eT_CurScene,
        eT_RecursiveScene,
        eT_CopyScene,

        eT_Refractive,
        eT_ForceRefractionUpdate,
        eT_Heat,

        eT_DepthBuffer,
        eT_DepthBufferTemp,
        eT_DepthBufferOrig,

        eT__ScreenSize,
        eT_WaterReflect,
        eT_FogColor,

        eT_Color,
        eT_Depth,

        eT__RT_2D,
        eT__RT_CM,
        eT__RT_Cube,

        eT_pass,
        eT_CustomRE,
        eT_Style,

        eT_VertexShader,
        eT_PixelShader,
        eT_GeometryShader,
        eT_HullShader,
        eT_DomainShader,
        eT_ComputeShader,
        eT_ZEnable,
        eT_ZWriteEnable,
        eT_CullMode,
        eT_SrcBlend,
        eT_DestBlend,
        eT_AlphaBlendEnable,
        eT_AlphaFunc,
        eT_AlphaRef,
        eT_ZFunc,
        eT_ColorWriteEnable,
        eT_IgnoreMaterialState,

        eT_None,
        eT_Disable,
        eT_CCW,
        eT_CW,
        eT_Back,
        eT_Front,

        eT_Never,
        eT_Less,
        eT_Equal,
        eT_LEqual,
        eT_LessEqual,
        eT_NotEqual,
        eT_GEqual,
        eT_GreaterEqual,
        eT_Greater,
        eT_Always,

        eT_RED,
        eT_GREEN,
        eT_BLUE,
        eT_ALPHA,

        eT_ONE,
        eT_ZERO,
        eT_SRC_COLOR,
        eT_SrcColor,
        eT_ONE_MINUS_SRC_COLOR,
        eT_InvSrcColor,
        eT_SRC_ALPHA,
        eT_SrcAlpha,
        eT_ONE_MINUS_SRC_ALPHA,
        eT_InvSrcAlpha,
        eT_DST_ALPHA,
        eT_DestAlpha,
        eT_ONE_MINUS_DST_ALPHA,
        eT_InvDestAlpha,
        eT_DST_COLOR,
        eT_DestColor,
        eT_ONE_MINUS_DST_COLOR,
        eT_InvDestColor,
        eT_SRC_ALPHA_SATURATE,

        eT_NULL,

        eT_cbuffer,
        eT_PER_BATCH,
        eT_PER_INSTANCE,
        eT_PER_MATERIAL,
        eT_SKIN_DATA,
        eT_INSTANCE_DATA,

        eT_ShaderType,
        eT_ShaderDrawType,
        eT_PreprType,
        eT_Public,
        eT_NoPreview,
        eT_LocalConstants,
        eT_Cull,
        eT_SupportsAttrInstancing,
        eT_SupportsConstInstancing,
        eT_SupportsDeferredShading,
        eT_SupportsFullDeferredShading,
        eT_Decal,
        eT_DecalNoDepthOffset,
        eT_NoChunkMerging,
        eT_ForceTransPass,
        eT_AfterHDRPostProcess,
        eT_AfterPostProcess,
        eT_ForceZpass,
        eT_ForceWaterPass,
        eT_ForceDrawLast,
        eT_ForceDrawFirst,
        eT_ForceDrawAfterWater,
        eT_DepthFixup,
        eT_DepthFixupReplace,
        eT_SingleLightPass,
        eT_HWTessellation,
        eT_WaterParticle,
        eT_AlphaBlendShadows,
        eT_ZPrePass,
        eT_WrinkleBlending,

        eT_Light,
        eT_Shadow,
        eT_Fur,
        eT_General,
        eT_Terrain,
        eT_Overlay,
        eT_NoDraw,
        eT_Custom,
        eT_Sky,
        eT_OceanShore,
        eT_Hair,
        eT_Compute,
        eT_ForceGeneralPass,
        eT_EyeOverlay,

        eT_Metal,
        eT_Ice,
        eT_Water,
        eT_FX,
        eT_HDR,
        eT_HUD3D,
        eT_Glass,
        eT_Vegetation,
        eT_Particle,
        eT_GenerateClouds,
        eT_ScanWater,

        eT_NoLights,
        eT_NoMaterialState,
        eT_PositionInvariant,
        eT_TechniqueZ,  // Has to be first Technique
        eT_TechniqueShadowGen,
        eT_TechniqueMotionBlur,
        eT_TechniqueCustomRender,
        eT_TechniqueEffectLayer,
        eT_TechniqueDebug,
        eT_TechniqueWaterRefl,
        eT_TechniqueWaterCaustic,
        eT_TechniqueZPrepass,
        eT_TechniqueThickness,

        eT_TechniqueMax,

        eT_KeyFrameParams,
        eT_KeyFrameRandColor,
        eT_KeyFrameRandIntensity,
        eT_KeyFrameRandSpecMult,
        eT_KeyFrameRandPosOffset,
        eT_Speed,

        eT_Beam,
        eT_LensOptics,
        eT_Cloud,
        eT_Ocean,

        eT_Model,
        eT_StartRadius,
        eT_EndRadius,
        eT_StartColor,
        eT_EndColor,
        eT_LightStyle,
        eT_Length,

        eT_RGBStyle,
        eT_Scale,
        eT_Blind,
        eT_SizeBlindScale,
        eT_SizeBlindBias,
        eT_IntensBlindScale,
        eT_IntensBlindBias,
        eT_MinLight,
        eT_DistFactor,
        eT_DistIntensityFactor,
        eT_FadeTime,
        eT_Layer,
        eT_Importance,
        eT_VisAreaScale,

        eT_Poly,
        eT_Identity,
        eT_FromObj,
        eT_FromLight,
        eT_Fixed,

        eT_ParticlesFile,

        eT_Gravity,
        eT_WindDirection,
        eT_WindSpeed,
        eT_WaveHeight,
        eT_DirectionalDependence,
        eT_ChoppyWaveFactor,
        eT_SuppressSmallWavesFactor,

        eT__LT_LIGHTS,
        eT__LT_NUM,
        eT__LT_HASPROJ,
        eT__LT_0_TYPE,
        eT__LT_1_TYPE,
        eT__LT_2_TYPE,
        eT__LT_3_TYPE,
        eT__TT_TEXCOORD_MATRIX,
        eT__TT_TEXCOORD_PROJ,
        eT__TT_TEXCOORD_GEN_OBJECT_LINEAR,
        eT__VT_TYPE,
        eT__VT_TYPE_MODIF,
        eT__VT_BEND,
        eT__VT_DET_BEND,
        eT__VT_GRASS,
        eT__VT_WIND,
        eT__VT_DEPTH_OFFSET,
        eT__FT_TEXTURE,
        eT__FT_TEXTURE1,
        eT__FT_NORMAL,
        eT__FT_PSIZE,
        eT__FT_DIFFUSE,
        eT__FT_SPECULAR,
        eT__FT_TANGENT_STREAM,
        eT__FT_QTANGENT_STREAM,
        eT__FT_SKIN_STREAM,
        eT__FT_VERTEX_VELOCITY_STREAM,
        eT__FT0_COP,
        eT__FT0_AOP,
        eT__FT0_CARG1,
        eT__FT0_CARG2,
        eT__FT0_AARG1,
        eT__FT0_AARG2,

        eT__VS,
        eT__PS,
        eT__GS,
        eT__HS,
        eT__DS,
        eT__CS,

        eT_x,
        eT_y,
        eT_z,
        eT_w,
        eT_r,
        eT_g,
        eT_b,
        eT_a,

        eT_true,
        eT_false,

        eT_0,
        eT_1,
        eT_2,
        eT_3,
        eT_4,
        eT_5,
        eT_6,
        eT_7,
        eT_8,
        eT_9,
        eT_10,
        eT_11,
        eT_12,
        eT_13,
        eT_14,
        eT_15,

        eT_AnisotropyLevel,

        eT_ORBIS,
        eT_DURANGO,
        eT_PCDX11,
        eT_PCDX12,
        eT_UNUSED,
        eT_VULKAN,

        eT_VT_DetailBendingGrass,
        eT_VT_DetailBending,
        eT_VT_WindBending,
        eT_VertexColors,

        eT_s0,
        eT_s1,
        eT_s2,
        eT_s3,
        eT_s4,
        eT_s5,
        eT_s6,
        eT_s7,
        eT_s8,
        eT_s9,
        eT_s10,
        eT_s11,
        eT_s12,
        eT_s13,
        eT_s14,
        eT_s15,

        eT_t0,
        eT_t1,
        eT_t2,
        eT_t3,
        eT_t4,
        eT_t5,
        eT_t6,
        eT_t7,
        eT_t8,
        eT_t9,
        eT_t10,
        eT_t11,
        eT_t12,
        eT_t13,
        eT_t14,
        eT_t15,

        eT_Global,

        eT_Load,
        eT_Sample,
        eT_Gather,
        eT_GatherRed,
        eT_GatherGreen,
        eT_GatherBlue,
        eT_GatherAlpha,

        eT__AutoGS_MultiRes,
        eT_Billboard,
        eT_DebugHelper,

        eT_max,
        eT_user_first = eT_max + 1
    }

    public enum ETokenStorageClass
    {
        eTS_invalid = 0,
        eTS_default,
        eTS_static,
        eTS_const,
        eTS_shared,
        eTS_groupshared
    }

    public struct SFXTokenBin
    {
        public uint id;
    }

    //#define FX_BEGIN_TOKENS static SFXTokenBin sCommands[] = {
    //#define FX_END_TOKENS { eT_unknown } };
    //#define FX_TOKEN(id) { Parser.fxTokenKey( # id, eT_ ## id) },
    //#define FX_REGISTER_TOKEN(id) fxTokenKey( # id, eT_ ## id);

    partial class G
    {
        internal string[] g_KeyTokens;
    }

    public class SMacroBinFX
    {
        public static SMacroBinFX Empty = new SMacroBinFX(); //: SKY
        public List<uint> m_Macro;
        public ulong m_nMask;
    }

    public struct TokenMask
    {
        public uint m_firstToken;
        public uint m_lastToken;
        public ulong m_mask;

        public TokenMask(uint firstToken, uint lastToken, ulong mask)
        {
            m_firstToken = firstToken;
            m_lastToken = lastToken;
            m_mask = mask;
        }
    }

    public struct SParserFrame
    {
        public uint m_nFirstToken;
        public uint m_nLastToken;
        public uint m_nCurToken;

        public SParserFrame(uint nFirstToken, uint nLastToken)
        {
            m_nFirstToken = nFirstToken;
            m_nLastToken = nLastToken;
            m_nCurToken = m_nFirstToken;
        }
        public SParserFrame(Exception e = null)
        {
            m_nFirstToken = 0;
            m_nLastToken = 0;
            m_nCurToken = m_nFirstToken;
        }
        public void Reset()
        {
            m_nFirstToken = 0;
            m_nLastToken = 0;
            m_nCurToken = m_nFirstToken;
        }
        public bool IsEmpty() => m_nFirstToken == 0 && m_nLastToken == 0
            ? true
            : m_nLastToken < m_nFirstToken;
    }

    public enum EFragmentType
    {
        eFT_Unknown,
        eFT_Function,
        eFT_Structure,
        eFT_Sampler,
        eFT_ConstBuffer,
        eFT_StorageClass
    }

    public struct SCodeFragment
    {
        uint m_nFirstToken;
        uint m_nLastToken;
        uint m_dwName;
        EFragmentType m_eType;
#if !_DEBUG
        //string m_Name;
#endif
        public SCodeFragment(Exception e = null)
        {
            m_nFirstToken = 0;
            m_nLastToken = 0;
            m_dwName = 0;
            m_eType = EFragmentType.eFT_Unknown;
        }
    }

    //public struct SortByToken
    //{
    //    public static bool operator()(STokenD left, STokenD right) => left.Token<right.Token;
    //    public static bool operator()(uint left, STokenD right) => left<right.Token;
    //    public static bool operator()(STokenD left, uint right) => left.Token<right;
    //}

    partial class G
    {
        public const uint SF_D3D12 = 0x02000000U; // SM5.1, SM6.0, SM6.1, SM6.2
        public const uint SF_VULKAN = 0x04000000U;
        public const uint SF_D3D11 = 0x10000000U; // SM5.0
        public const uint SF_ORBIS = 0x20000000U;
        public const uint SF_DURANGO = 0x40000000U;
        public const uint SF_PLATFORM = 0xfC000000U;
    }

    public partial class CParserBin
    {
        //bool m_bEmbeddedSearchInfo;
        SShaderBin m_pCurBinShader;
        CShader m_pCurShader;
        uint[] m_Tokens;
        FXMacroBin[] m_Macros = new FXMacroBin[3];
        FXShaderToken m_TokenTable;
        ulong[] m_IfAffectMask;
        TokenMask[] m_tokenMasks;
        //List<List<int>> m_KeyOffsets;
        EToken m_eToken;
        uint m_nFirstToken;
        SCodeFragment[] m_CodeFragments;

        SParserFrame m_CurFrame;

        SParserFrame m_Name;
        SParserFrame m_Assign;
        SParserFrame m_Annotations;
        SParserFrame m_Value;
        SParserFrame m_Data;

        static FXMacroBin m_StaticMacros;

        public CParserBin(SShaderBin pBin)
        {
            m_pCurBinShader = pBin;
            m_pCurShader = null;
        }
        public CParserBin(SShaderBin pBin, CShader pSH)
        {
            m_pCurBinShader = pBin;
            m_pCurShader = pSH;
        }

        public static FXMacroBin GetStaticMacroses() => m_StaticMacros;

        public static string GetString(uint nToken, FXShaderToken Table, bool bOnlyKey = false)
        {
            if (nToken < (uint)EToken.eT_max)
            {
                Debug.Assert(g_KeyTokens[nToken] != null);
                return g_KeyTokens[nToken];
            }
            if (!bOnlyKey)
            {
                var it = Table.FirstOrDefault(x => x.Token == nToken); //: SKY // std::lower_bound(Table.begin(), Table.end(), nToken, SortByToken());
                if (it.SToken != null && it.Token == nToken)
                    return it.SToken;
            }

            Debug.Assert(false);
            return "";
        }

        public string GetString(uint nToken, bool bOnlyKey = false) => GetString(nToken, m_TokenTable, bOnlyKey);

        public string GetString(SParserFrame Frame)
        {
            if (Frame.IsEmpty())
                return "";

            StringBuilder Str = new StringBuilder();
            int nCur = (int)Frame.m_nFirstToken;
            int nLast = (int)Frame.m_nLastToken;
            while (nCur <= nLast)
            {
                uint nTok = m_Tokens[nCur++];
                string szStr = GetString(nTok);
                if (Str.Length != 0 && !Parser.SkipChar(Str[Str.Length - 1]) && !Parser.SkipChar(szStr[0]))
                    Str.Append(" ");
                Str.Append(szStr);
            }
            return Str.ToString();
        }

        public string GetNameString(SParserFrame Frame)
        {
            if (Frame.IsEmpty())
                return "";

            StringBuilder Str = new StringBuilder();
            int nCur = (int)Frame.m_nFirstToken;
            int nLast = (int)Frame.m_nLastToken;
            while (nCur <= nLast)
            {
                uint nTok = m_Tokens[nCur++];
                string szStr = GetString(nTok);
                if (Str.Length != 0 && !Parser.SkipChar(Str[Str.Length - 1]) && !Parser.SkipChar(szStr[0]))
                    Str.Append(" ");
                Str.Append(szStr);
            }
            return Str.ToString();
        }

        public void BuildSearchInfo() => throw new NotImplementedException();
        public bool PreprocessTokens(ShaderTokensVec Tokens, int nPass, uint[] tokensBuffer) => throw new NotImplementedException();
        public bool Preprocess(int nPass, ShaderTokensVec Tokens, FXShaderToken pSrcTable) => throw new NotImplementedException();

        public static SMacroBinFX FindMacro(uint dwName, FXMacroBin Macro)
        {
            if (Macro.TryGetValue(dwName, out var it))
                return it;
            return null;
        }

        public static bool AddMacro(uint dwToken, uint[] pMacro, int nMacroTokens, ulong nMask, FXMacroBin Macro)
        {
            if (!Macro.TryGetValue(dwToken, out var macro))
                Macro.Add(dwToken, macro = new SMacroBinFX());
            macro.m_nMask = nMask;
            if (nMacroTokens != 0)
            {
                Debug.Assert(pMacro.Length == nMacroTokens);
                macro.m_Macro.Clear();
                macro.m_Macro.AddRange(pMacro);
            }
            else
                macro.m_Macro.Clear();
            return true;
        }

        public static bool RemoveMacro(uint dwToken, FXMacroBin Macro)
        {
            if (Macro.ContainsKey(dwToken))
                return false;
            else
                Macro.Remove(dwToken);
            return true;
        }

        public static void CleanPlatformMacros()
        {
            RemoveMacro(CParserBin.fxToken("DURANGO", out var dummy), m_StaticMacros);
            RemoveMacro(CParserBin.fxToken("ORBIS", out dummy), m_StaticMacros);
            RemoveMacro(CParserBin.fxToken("PCDX11", out dummy), m_StaticMacros);
            RemoveMacro(CParserBin.fxToken("VULKAN", out dummy), m_StaticMacros);
        }

        public uint NewUserToken(uint nToken, string psToken, bool bUseFinalTable)
        {
            if (nToken != (uint)EToken.eT_unknown)
                return nToken;
            nToken = GetCRC32(psToken);

            if (bUseFinalTable)
            {
                var itor = m_TokenTable.FirstOrDefault(x => x.Token == nToken); //: SKY // std::lower_bound(m_TokenTable.begin(), m_TokenTable.end(), nToken, SortByToken());
                if (itor.SToken != null && itor.Token == nToken)
                {
                    Debug.Assert(itor.SToken == psToken);
                    return nToken;
                }
                STokenD TD;
                TD.SToken = psToken;
                TD.Token = nToken;
                m_TokenTable.Add(TD);
            }
            else
            {
                SShaderBin pBin = m_pCurBinShader;
                Debug.Assert(pBin != null);
                var itor = pBin.m_TokenTable.FirstOrDefault(x => x.Token == nToken); //: SKY // std::lower_bound(pBin->m_TokenTable.begin(), pBin->m_TokenTable.end(), nToken, SortByToken());
                if (itor.SToken != null && itor.Token == nToken)
                {
                    Debug.Assert(itor.SToken == psToken);
                    return nToken;
                }
                STokenD TD;
                TD.SToken = psToken;
                TD.Token = nToken;
                pBin.m_TokenTable.Add(TD);
            }

            return nToken;
        }


        //uint32 NewUserToken(uint32 nToken, const string& sToken, bool bUseFinalTable) => throw new NotImplementedException();

        public void MergeTable(SShaderBin pBin)
        {
            FXShaderTokenItor it = m_TokenTable.begin();
            FXShaderTokenItor end = m_TokenTable.end();
            FXShaderTokenItor bit = pBin.m_TokenTable.begin();
            FXShaderTokenItor bend = pBin.m_TokenTable.end();

            FXShaderToken newTable;
            newTable.reserve(m_TokenTable.size() + pBin->m_TokenTable.size());
            while (true)
            {
                STokenD last = newTable.size() ? &(*newTable.rbegin()) : NULL;

                uint mask = 0;
                mask |= (bit != bend) << 0;
                mask |= (it != end) << 1;

                switch (mask)
                {
                    // No iterators valid anymore, nothing left to do
                    case 0x0:
                        goto done;

                    // Other iterator valid, internal iterator invalid
                    case 0x1:
                        if (!last || bit->Token != last->Token)
                            newTable.push_back(*bit);
                        ++bit;
                        break;

                    // Other iterator invalid, internal iterator valid
                    case 0x2:
                        if (!last || it->Token != last->Token)
                            newTable.push_back(*it);
                        ++it;
                        break;

                    // Noth iterators valid
                    case 0x3:
                        {
                            STokenD & iTD = (*it);
                            STokenD & oTD = (*bit);
                            if (iTD.Token < oTD.Token)
                            {
                                if (!last || it->Token != last->Token)
                                    newTable.push_back(*it);
                                ++it;
                            }
                            else
                            {
                                if (!last || bit->Token != last->Token)
                                    newTable.push_back(*bit);
                                ++bit;
                            }
                        }
                        break;
                }
                ;
            }

        // Verify that the merging results in a sorted table
#if defined _DEBUG
	for (std::size_t i = 1; i < newTable.size(); ++i)
	{
		assert(newTable[i - 1].Token <= newTable[i].Token);
	}
#endif

        done:
            swap(newTable, m_TokenTable);
        }

        public bool CheckIfExpression(uint pTokens, uint nT, int nPass, ulong nMask = 0) => throw new NotImplementedException();

        public bool IgnorePreprocessBlock(uint pTokens, uint nT, int nMaxTokens, uint[] tokensBuffer, int nPass) => throw new NotImplementedException();

        static void sCR(List<byte> Text, int nLevel)
        {
            Text.Add((byte)'\n');
            for (int i = 0; i < nLevel; i++)
            {
                Text.Add((byte)' ');
                Text.Add((byte)' ');
            }
        }

        public static bool CorrectScript(uint[] pTokens, uint i, uint nT, List<byte> Text)
        {
            int nTex = Text.Count - 1;
            int nTT = nTex;
            while (nTex > 0)
            {
                byte c = Text[nTex];
                if (c <= 32)
                {
                    nTex++;
                    break;
                }
                nTex--;
            }
            var newText = new byte[5];
            Text.CopyTo(nTex, newText, 0, 5);
            if (Encoding.ASCII.GetString(newText) == "float")
            {
                Debug.Assert(false);
                Core.iLog.Log("Wrong script tokens...");
                return false;
            }
            Text.Memset(nTex, 0x20, nTT - nTex + 1);
            i++;
            while (i < nT)
            {
                uint nTok = pTokens[(int)i];
                if (nTok == (uint)EToken.eT_semicolumn)
                    return true;
                i++;
            }
            return false;
        }

        public static bool ConvertToAscii(uint[] pTokens, uint nT, FXShaderToken Table, List<byte> Text, bool bInclSkipTokens = false)
        {
            uint i;
            bool bRes = true;

            int nLevel = 0;
            for (i = 0; i < nT; i++)
            {
                uint nToken = pTokens[i];
                if (nToken == 0)
                {
                    Text.Add((byte)'\n');
                    continue;
                }
                if (!bInclSkipTokens)
                {
                    if (nToken == (uint)EToken.eT_skip)
                    {
                        i++;
                        continue;
                    }
                    if (nToken == (uint)EToken.eT_skip_1)
                    {
                        while (i < nT)
                        {
                            nToken = pTokens[i];
                            if (nToken == (uint)EToken.eT_skip_2)
                                break;
                            i++;
                        }
                        Debug.Assert(i < nT);
                        continue;
                    }
                }
                string szStr = GetString(nToken, Table, false);
                Debug.Assert(szStr != null);
                if (szStr[0] == 0)
                {
                    bRes = CParserBin.CorrectScript(pTokens, i, nT, Text);
                }
                else
                {
                    if (nToken == (uint)EToken.eT_semicolumn || nToken == (uint)EToken.eT_br_cv_1)
                    {
                        if (nToken == (uint)EToken.eT_br_cv_1)
                        {
                            sCR(Text, nLevel);
                            nLevel++;
                        }
                        Text.AddRange(Encoding.ASCII.GetBytes(szStr));
                        if (nToken == (uint)EToken.eT_semicolumn)
                        {
                            if (i + 1 < nT && pTokens[i + 1] == (uint)EToken.eT_br_cv_2)
                                sCR(Text, nLevel - 1);
                            else
                                sCR(Text, nLevel);
                        }
                        else if (i + 1 < nT)
                        {
                            if (pTokens[i + 1] < (uint)EToken.eT_br_rnd_1 || pTokens[i + 1] >= (uint)EToken.eT_float)
                                sCR(Text, nLevel);
                        }
                    }
                    else
                    {
                        if (i + 1 < nT)
                        {
                            if (Text.Count != 0)
                            {
                                byte cPrev = Text[Text.Count - 1];
                                if (!Parser.SkipChar(cPrev) && !Parser.SkipChar(szStr[0]))
                                    Text.Add((byte)' ');
                            }
                        }
                        Text.AddRange(Encoding.ASCII.GetBytes(szStr));
                        if (nToken == (uint)EToken.eT_br_cv_2)
                        {
                            nLevel--;
                            if (i + 1 < nT && pTokens[i + 1] != (uint)EToken.eT_semicolumn)
                                sCR(Text, nLevel);
                        }
                    }
                }
            }
            Text.Add(0);

            return bRes;
        }

        public bool GetBool(SParserFrame Frame)
        {
            if (Frame.IsEmpty())
                return true;
            EToken eT = GetToken(Frame);
            if (eT == EToken.eT_true || eT == EToken.eT_1)
                return true;
            if (eT == EToken.eT_false || eT == EToken.eT_0)
                return false;
            Debug.Assert(false);
            return false;
        }

        public (uint[], int) GetTokens(int nStart) { return (m_Tokens, nStart); }

        public int GetNumTokens() => m_Tokens.Length;

        public EToken GetToken() => m_eToken;

        public EToken GetToken(SParserFrame Frame)
        {
            Debug.Assert(!Frame.IsEmpty());
            return (EToken)m_Tokens[Frame.m_nFirstToken];
        }

        public uint FirstToken() => m_nFirstToken;

        public int GetInt(uint nToken)
        {
            string szStr = GetString(nToken);
            return szStr[0] == '0' && szStr[1] == 'x'
                ? G.VERIFY<int>(() => Convert.ToInt32(szStr, 16))
                : int.Parse(szStr);
        }

        public float GetFloat(SParserFrame Frame) => float.Parse(GetString(Frame));

        public static uint NextToken(uint[] pTokens, uint nCur, uint nLast)
        {
            while (nCur <= nLast)
            {
                var nToken = pTokens[nCur++];
                if (nToken == (uint)EToken.eT_skip)
                {
                    nCur++;
                    continue;
                }
                if (nToken == (uint)EToken.eT_skip_1)
                {
                    while (nCur <= nLast)
                    {
                        nToken = pTokens[nCur++];
                        if (nToken == (uint)EToken.eT_skip_2)
                            break;
                    }
                    continue;
                }
                return nToken;
            }
            return 0;
        }

        public ulong GetTokenMask(uint token)
        {
            foreach (var tokenMask in m_tokenMasks)
            {
                if (tokenMask.m_firstToken <= token && tokenMask.m_lastToken >= token)
                    return tokenMask.m_mask;
            }
            return 0;
        }

        public SParserFrame BeginFrame(SParserFrame Frame) => throw new NotImplementedException();
        public void EndFrame(SParserFrame Frame) => throw new NotImplementedException();

        public byte GetCompareFunc(EToken eT) => throw new NotImplementedException();
        public int GetSrcBlend(EToken eT) => throw new NotImplementedException();
        public int GetDstBlend(EToken eT) => throw new NotImplementedException();

        public void InsertSkipTokens(uint[] pTokens, uint nStart, uint nTokens, bool bSingle, uint[] tokensBuffer) => throw new NotImplementedException();
        public ETokenStorageClass ParseObject(SFXTokenBin pTokens, int nIndex) => throw new NotImplementedException();
        public ETokenStorageClass ParseObject(SFXTokenBin pTokens) => throw new NotImplementedException();
        public int GetNextToken(ref uint nStart, ETokenStorageClass nTokenStorageClass) => throw new NotImplementedException();
        public bool FXGetAssignmentData(SParserFrame Frame) => throw new NotImplementedException();
        public bool FXGetAssignmentData2(SParserFrame Frame) => throw new NotImplementedException();
        public bool GetAssignmentData(SParserFrame Frame) => throw new NotImplementedException();
        public bool GetSubData(SParserFrame Frame, EToken eT1, EToken eT2) => throw new NotImplementedException();
        public static int FindToken(uint nStart, uint nLast, uint[] pTokens, uint nToken) => throw new NotImplementedException();
        public int FindToken(uint nStart, uint nLast, uint nToken) => throw new NotImplementedException();
        public int FindToken(uint nStart, uint nLast, uint[] pTokens) => throw new NotImplementedException();
        public int CopyTokens(SParserFrame Fragment, List<uint> NewTokens) => throw new NotImplementedException();
        public int CopyTokens(SCodeFragment pCF, uint[] SHData, SCodeFragment[] Replaces, uint[] NewTokens, uint nID) => throw new NotImplementedException();
        public static void AddDefineToken(uint dwToken, ShaderTokensVec Tokens)
        {
            Tokens.Add((uint)EToken.eT_define);
            Tokens.Add(dwToken);
            Tokens.Add(0);
        }
        public static void AddDefineToken(uint dwToken, uint dwToken2, ShaderTokensVec Tokens)
        {
            Tokens.Add((uint)EToken.eT_define);
            Tokens.Add(dwToken);
            Tokens.Add(dwToken2);
            Tokens.Add(0);
        }
        public bool JumpSemicolumn(uint nStart, uint nEnd) => throw new NotImplementedException();

        public static uint fxToken(string szToken, out bool bKey)
        {
            for (int i = 0; i < (int)EToken.eT_max; i++)
            {
                if (g_KeyTokens[i] == null)
                    continue;
                if (string.Equals(szToken, g_KeyTokens[i]))
                {
                    bKey = true;
                    return (uint)i;
                }
            }
            bKey = false;
            return (uint)EToken.eT_unknown;
        }
        public static uint fxTokenKey(string szToken, EToken eT = EToken.eT_unknown)
        {
            g_KeyTokens[(int)eT] = szToken;
            return (uint)eT;
        }
        public static uint GetCRC32(string szStr) => GetCRC32(Encoding.ASCII.GetBytes(szStr));
        public static uint GetCRC32(byte[] szStr)
        {
            var nGen = CCrc32.Compute(szStr, 0);
            Debug.Assert(nGen >= (int)EToken.eT_user_first);
            return nGen;
        }

        public static uint NextToken((string b, int i) buf, byte[] com, out bool bKey)
        {
            char ch;
            int n = 0;
            while ((ch = buf.b[buf.i]) != 0)
            {
                if (Parser.SkipChar(ch))
                    break;
                com[n++] = (byte)ch;
                ++buf.i;
                if (ch == '/')
                    break;
            }
            if (n == 0)
                if (ch != ' ')
                {
                    com[n++] = (byte)ch;
                    ++buf.i;
                }
            com[n] = 0;
            uint dwToken = fxToken(Encoding.ASCII.GetString(com), out bKey);
            return dwToken;
        }

        public static void Init()
        {
            // Register key tokens
            fxTokenKey("#include", EToken.eT_include);
            fxTokenKey("#define", EToken.eT_define);
            fxTokenKey("#undefine", EToken.eT_undefine);
            fxTokenKey("#define", EToken.eT_define_2);
            fxTokenKey("#fetchinst", EToken.eT_fetchinst);
            fxTokenKey("#if", EToken.eT_if);
            fxTokenKey("#ifdef", EToken.eT_ifdef);
            fxTokenKey("#ifndef", EToken.eT_ifndef);
            fxTokenKey("#if", EToken.eT_if_2);
            fxTokenKey("#ifdef", EToken.eT_ifdef_2);
            fxTokenKey("#ifndef", EToken.eT_ifndef_2);
            fxTokenKey("#endif", EToken.eT_endif);
            fxTokenKey("#else", EToken.eT_else);
            fxTokenKey("#elif", EToken.eT_elif);
            fxTokenKey("#elif", EToken.eT_elif_2);
            fxTokenKey("#warning", EToken.eT_warning);
            fxTokenKey("#register_env", EToken.eT_register_env);
            fxTokenKey("#ifcvar", EToken.eT_ifcvar);
            fxTokenKey("#ifncvar", EToken.eT_ifncvar);
            fxTokenKey("#elifcvar", EToken.eT_elifcvar);
            fxTokenKey("#skip", EToken.eT_skip);
            fxTokenKey("#skip_(", EToken.eT_skip_1);
            fxTokenKey("#skip_)", EToken.eT_skip_2);

            fxTokenKey("|", EToken.eT_or);
            fxTokenKey("&", EToken.eT_and);

            fxTokenKey("(", EToken.eT_br_rnd_1);
            fxTokenKey(")", EToken.eT_br_rnd_2);
            fxTokenKey("[", EToken.eT_br_sq_1);
            fxTokenKey("]", EToken.eT_br_sq_2);
            fxTokenKey("{", EToken.eT_br_cv_1);
            fxTokenKey("}", EToken.eT_br_cv_2);
            fxTokenKey("<", EToken.eT_br_tr_1);
            fxTokenKey(">", EToken.eT_br_tr_2);
            fxTokenKey(",", EToken.eT_comma);
            fxTokenKey(".", EToken.eT_dot);
            fxTokenKey(":", EToken.eT_colon);
            fxTokenKey(";", EToken.eT_semicolumn);
            fxTokenKey("!", EToken.eT_excl);
            fxTokenKey("\"", EToken.eT_quote);
            fxTokenKey("'", EToken.eT_sing_quote);

            fxTokenKey("s0", EToken.eT_s0);
            fxTokenKey("s1", EToken.eT_s1);
            fxTokenKey("s2", EToken.eT_s2);
            fxTokenKey("s3", EToken.eT_s3);
            fxTokenKey("s4", EToken.eT_s4);
            fxTokenKey("s5", EToken.eT_s5);
            fxTokenKey("s6", EToken.eT_s6);
            fxTokenKey("s7", EToken.eT_s7);
            fxTokenKey("s8", EToken.eT_s8);
            fxTokenKey("s9", EToken.eT_s9);
            fxTokenKey("s10", EToken.eT_s10);
            fxTokenKey("s11", EToken.eT_s11);
            fxTokenKey("s12", EToken.eT_s12);
            fxTokenKey("s13", EToken.eT_s13);
            fxTokenKey("s14", EToken.eT_s14);
            fxTokenKey("s15", EToken.eT_s15);

            fxTokenKey("t0", EToken.eT_t0);
            fxTokenKey("t1", EToken.eT_t1);
            fxTokenKey("t2", EToken.eT_t2);
            fxTokenKey("t3", EToken.eT_t3);
            fxTokenKey("t4", EToken.eT_t4);
            fxTokenKey("t5", EToken.eT_t5);
            fxTokenKey("t6", EToken.eT_t6);
            fxTokenKey("t7", EToken.eT_t7);
            fxTokenKey("t8", EToken.eT_t8);
            fxTokenKey("t9", EToken.eT_t9);
            fxTokenKey("t10", EToken.eT_t10);
            fxTokenKey("t11", EToken.eT_t11);
            fxTokenKey("t12", EToken.eT_t12);
            fxTokenKey("t13", EToken.eT_t13);
            fxTokenKey("t14", EToken.eT_t14);
            fxTokenKey("t15", EToken.eT_t15);

            fxTokenKey("//", EToken.eT_comment);

            fxTokenKey("?", EToken.eT_question);
            fxTokenKey("=", EToken.eT_eq);
            fxTokenKey("+", EToken.eT_plus);
            fxTokenKey("-", EToken.eT_minus);
            fxTokenKey("/", EToken.eT_div);
            fxTokenKey("*", EToken.eT_mul);
            fxTokenKey("dot", EToken.eT_dot_math);
            fxTokenKey("mul", EToken.eT_mul_math);
            fxTokenKey("sqrt", EToken.eT_sqrt_math);
            fxTokenKey("exp", EToken.eT_exp_math);
            fxTokenKey("log", EToken.eT_log_math);
            fxTokenKey("log2", EToken.eT_log2_math);
            fxTokenKey("sin", EToken.eT_sin_math);
            fxTokenKey("cos", EToken.eT_cos_math);
            fxTokenKey("sincos", EToken.eT_sincos_math);
            fxTokenKey("floor", EToken.eT_floor_math);
            fxTokenKey("floor", EToken.eT_ceil_math);
            fxTokenKey("frac", EToken.eT_frac_math);
            fxTokenKey("lerp", EToken.eT_lerp_math);
            fxTokenKey("abs", EToken.eT_abs_math);
            fxTokenKey("clamp", EToken.eT_clamp_math);
            fxTokenKey("min", EToken.eT_min_math);
            fxTokenKey("max", EToken.eT_max_math);
            fxTokenKey("length", EToken.eT_length_math);

            fxTokenKey("%_LT_LIGHTS", EToken.eT__LT_LIGHTS);
            fxTokenKey("%_LT_NUM", EToken.eT__LT_NUM);
            fxTokenKey("%_LT_HASPROJ", EToken.eT__LT_HASPROJ);
            fxTokenKey("%_LT_0_TYPE", EToken.eT__LT_0_TYPE);
            fxTokenKey("%_LT_1_TYPE", EToken.eT__LT_1_TYPE);
            fxTokenKey("%_LT_2_TYPE", EToken.eT__LT_2_TYPE);
            fxTokenKey("%_LT_3_TYPE", EToken.eT__LT_3_TYPE);
            fxTokenKey("%_TT_TEXCOORD_MATRIX", EToken.eT__TT_TEXCOORD_MATRIX);
            fxTokenKey("%_TT_TEXCOORD_GEN_OBJECT_LINEAR", EToken.eT__TT_TEXCOORD_GEN_OBJECT_LINEAR);
            fxTokenKey("%_TT_TEXCOORD_PROJ", EToken.eT__TT_TEXCOORD_PROJ);
            fxTokenKey("%_VT_TYPE", EToken.eT__VT_TYPE);
            fxTokenKey("%_VT_TYPE_MODIF", EToken.eT__VT_TYPE_MODIF);
            fxTokenKey("%_VT_BEND", EToken.eT__VT_BEND);
            fxTokenKey("%_VT_DET_BEND", EToken.eT__VT_DET_BEND);
            fxTokenKey("%_VT_GRASS", EToken.eT__VT_GRASS);
            fxTokenKey("%_VT_WIND", EToken.eT__VT_WIND);
            fxTokenKey("%_VT_DEPTH_OFFSET", EToken.eT__VT_DEPTH_OFFSET);
            fxTokenKey("%_FT_TEXTURE", EToken.eT__FT_TEXTURE);
            fxTokenKey("%_FT_TEXTURE1", EToken.eT__FT_TEXTURE1);
            fxTokenKey("%_FT_NORMAL", EToken.eT__FT_NORMAL);
            fxTokenKey("%_FT_PSIZE", EToken.eT__FT_PSIZE);
            fxTokenKey("%_FT_DIFFUSE", EToken.eT__FT_DIFFUSE);
            fxTokenKey("%_FT_SPECULAR", EToken.eT__FT_SPECULAR);
            fxTokenKey("%_FT_TANGENT_STREAM", EToken.eT__FT_TANGENT_STREAM);
            fxTokenKey("%_FT_QTANGENT_STREAM", EToken.eT__FT_QTANGENT_STREAM);
            fxTokenKey("%_FT_SKIN_STREAM", EToken.eT__FT_SKIN_STREAM);
            fxTokenKey("%_FT_VERTEX_VELOCITY_STREAM", EToken.eT__FT_VERTEX_VELOCITY_STREAM);
            fxTokenKey("%_FT0_COP", EToken.eT__FT0_COP);
            fxTokenKey("%_FT0_AOP", EToken.eT__FT0_AOP);
            fxTokenKey("%_FT0_CARG1", EToken.eT__FT0_CARG1);
            fxTokenKey("%_FT0_CARG2", EToken.eT__FT0_CARG2);
            fxTokenKey("%_FT0_AARG1", EToken.eT__FT0_AARG1);
            fxTokenKey("%_FT0_AARG2", EToken.eT__FT0_AARG2);

            fxTokenKey("%_VS", EToken.eT__VS);
            fxTokenKey("%_PS", EToken.eT__PS);
            fxTokenKey("%_GS", EToken.eT__GS);
            fxTokenKey("%_HS", EToken.eT__HS);
            fxTokenKey("%_DS", EToken.eT__DS);
            fxTokenKey("%_CS", EToken.eT__CS);

            // FX_REGISTER_TOKEN
            fxTokenKey("tex2D", EToken.eT_tex2D);
            fxTokenKey("tex2Dproj", EToken.eT_tex2Dproj);
            fxTokenKey("tex3D", EToken.eT_tex3D);
            fxTokenKey("texCUBE", EToken.eT_texCUBE);
            fxTokenKey("sampler1D", EToken.eT_sampler1D);
            fxTokenKey("sampler2D", EToken.eT_sampler2D);
            fxTokenKey("sampler3D", EToken.eT_sampler3D);
            fxTokenKey("samplerCUBE", EToken.eT_samplerCUBE);
            fxTokenKey("SamplerState", EToken.eT_SamplerState);
            fxTokenKey("SamplerComparisonState", EToken.eT_SamplerComparisonState);
            fxTokenKey("sampler_state", EToken.eT_sampler_state);
            fxTokenKey("Texture2D", EToken.eT_Texture2D);
            fxTokenKey("Texture2DArray", EToken.eT_Texture2DArray);
            fxTokenKey("Texture2DMS", EToken.eT_Texture2DMS);
            fxTokenKey("RWTexture2D", EToken.eT_RWTexture2D);
            fxTokenKey("RWTexture2DArray", EToken.eT_RWTexture2DArray);
            fxTokenKey("TextureCube", EToken.eT_TextureCube);
            fxTokenKey("TextureCubeArray", EToken.eT_TextureCubeArray);
            fxTokenKey("Texture3D", EToken.eT_Texture3D);
            fxTokenKey("RWTexture3D", EToken.eT_RWTexture3D);

            fxTokenKey("unorm", EToken.eT_unorm);
            fxTokenKey("snorm", EToken.eT_snorm);
            fxTokenKey("float", EToken.eT_float);
            fxTokenKey("float2", EToken.eT_float2);
            fxTokenKey("float3", EToken.eT_float3);
            fxTokenKey("float4", EToken.eT_float4);
            fxTokenKey("float2x4", EToken.eT_float2x4);
            fxTokenKey("float3x4", EToken.eT_float3x4);
            fxTokenKey("float4x4", EToken.eT_float4x4);
            fxTokenKey("float3x3", EToken.eT_float3x3);
            fxTokenKey("half", EToken.eT_half);
            fxTokenKey("half2", EToken.eT_half2);
            fxTokenKey("half3", EToken.eT_half3);
            fxTokenKey("half4", EToken.eT_half4);
            fxTokenKey("half2x4", EToken.eT_half2x4);
            fxTokenKey("half3x4)", EToken.eT_half3x4);
            fxTokenKey("half4x4", EToken.eT_half4x4);
            fxTokenKey("half3x3", EToken.eT_half3x3);
            fxTokenKey("bool", EToken.eT_bool);
            fxTokenKey("int", EToken.eT_int);
            fxTokenKey("int2", EToken.eT_int2);
            fxTokenKey("int3", EToken.eT_int3);
            fxTokenKey("int4", EToken.eT_int4);
            fxTokenKey("uint", EToken.eT_uint);
            fxTokenKey("uint2", EToken.eT_uint2);
            fxTokenKey("uint3", EToken.eT_uint3);
            fxTokenKey("uint4", EToken.eT_uint4);
            fxTokenKey("min16float", EToken.eT_min16float);
            fxTokenKey("min16float2", EToken.eT_min16float2);
            fxTokenKey("min16float3", EToken.eT_min16float3);
            fxTokenKey("min16float4", EToken.eT_min16float4);
            fxTokenKey("min16float4x4", EToken.eT_min16float4x4);
            fxTokenKey("min16float3x4", EToken.eT_min16float3x4);
            fxTokenKey("min16float2x4", EToken.eT_min16float2x4);
            fxTokenKey("min16float3x3", EToken.eT_min16float3x3);
            fxTokenKey("min10float", EToken.eT_min10float);
            fxTokenKey("min10float2", EToken.eT_min10float2);
            fxTokenKey("min10float3", EToken.eT_min10float3);
            fxTokenKey("min10float4", EToken.eT_min10float4);
            fxTokenKey("min10float4x4", EToken.eT_min10float4x4);
            fxTokenKey("min10float3x4", EToken.eT_min10float3x4);
            fxTokenKey("min10float2x4", EToken.eT_min10float2x4);
            fxTokenKey("min10float3x3", EToken.eT_min10float3x3);
            fxTokenKey("min16int", EToken.eT_min16int);
            fxTokenKey("min16int2", EToken.eT_min16int2);
            fxTokenKey("min16int3", EToken.eT_min16int3);
            fxTokenKey("min16int4", EToken.eT_min16int4);
            fxTokenKey("min12int", EToken.eT_min12int);
            fxTokenKey("min12int2", EToken.eT_min12int2);
            fxTokenKey("min12int3", EToken.eT_min12int3);
            fxTokenKey("min12int4", EToken.eT_min12int4);
            fxTokenKey("min16uint", EToken.eT_min16uint);
            fxTokenKey("min16uint2", EToken.eT_min16uint2);
            fxTokenKey("min16uint3", EToken.eT_min16uint3);
            fxTokenKey("min16uint4", EToken.eT_min16uint4);

            fxTokenKey("inout", EToken.eT_inout);
            fxTokenKey("asm", EToken.eT_asm);

            fxTokenKey("struct", EToken.eT_struct);
            fxTokenKey("sampler", EToken.eT_sampler);
            fxTokenKey("const", EToken.eT_const);
            fxTokenKey("static", EToken.eT_static);
            fxTokenKey("groupshared", EToken.eT_groupshared);
            fxTokenKey("TEXCOORDN", EToken.eT_TEXCOORDN);
            fxTokenKey("TEXCOORD0", EToken.eT_TEXCOORD0);
            fxTokenKey("TEXCOORD1", EToken.eT_TEXCOORD1);
            fxTokenKey("TEXCOORD2", EToken.eT_TEXCOORD2);
            fxTokenKey("TEXCOORD3", EToken.eT_TEXCOORD3);
            fxTokenKey("TEXCOORD4", EToken.eT_TEXCOORD4);
            fxTokenKey("TEXCOORD5", EToken.eT_TEXCOORD5);
            fxTokenKey("TEXCOORD6", EToken.eT_TEXCOORD6);
            fxTokenKey("TEXCOORD7", EToken.eT_TEXCOORD7);
            fxTokenKey("TEXCOORD8", EToken.eT_TEXCOORD8);
            fxTokenKey("TEXCOORD9", EToken.eT_TEXCOORD9);
            fxTokenKey("TEXCOORD10", EToken.eT_TEXCOORD10);
            fxTokenKey("TEXCOORD11", EToken.eT_TEXCOORD11);
            fxTokenKey("TEXCOORD12", EToken.eT_TEXCOORD12);
            fxTokenKey("TEXCOORD13", EToken.eT_TEXCOORD13);
            fxTokenKey("TEXCOORD14", EToken.eT_TEXCOORD14);
            fxTokenKey("TEXCOORD15", EToken.eT_TEXCOORD15);
            fxTokenKey("TEXCOORD16", EToken.eT_TEXCOORD16);
            fxTokenKey("TEXCOORD17", EToken.eT_TEXCOORD17);
            fxTokenKey("TEXCOORD18", EToken.eT_TEXCOORD18);
            fxTokenKey("TEXCOORD19", EToken.eT_TEXCOORD19);
            fxTokenKey("TEXCOORD20", EToken.eT_TEXCOORD20);
            fxTokenKey("TEXCOORD21", EToken.eT_TEXCOORD21);
            fxTokenKey("TEXCOORD22", EToken.eT_TEXCOORD22);
            fxTokenKey("TEXCOORD23", EToken.eT_TEXCOORD23);
            fxTokenKey("TEXCOORD24", EToken.eT_TEXCOORD24);
            fxTokenKey("TEXCOORD25", EToken.eT_TEXCOORD25);
            fxTokenKey("TEXCOORD26", EToken.eT_TEXCOORD26);
            fxTokenKey("TEXCOORD27", EToken.eT_TEXCOORD27);
            fxTokenKey("TEXCOORD28", EToken.eT_TEXCOORD28);
            fxTokenKey("TEXCOORD29", EToken.eT_TEXCOORD29);
            fxTokenKey("TEXCOORD30", EToken.eT_TEXCOORD30);
            fxTokenKey("TEXCOORD31", EToken.eT_TEXCOORD31);
            fxTokenKey("TEXCOORDN_centroid", EToken.eT_TEXCOORDN_centroid);
            fxTokenKey("TEXCOORD0_centroid", EToken.eT_TEXCOORD0_centroid);
            fxTokenKey("TEXCOORD1_centroid", EToken.eT_TEXCOORD1_centroid);
            fxTokenKey("TEXCOORD2_centroid", EToken.eT_TEXCOORD2_centroid);
            fxTokenKey("TEXCOORD3_centroid", EToken.eT_TEXCOORD3_centroid);
            fxTokenKey("TEXCOORD4_centroid", EToken.eT_TEXCOORD4_centroid);
            fxTokenKey("TEXCOORD5_centroid", EToken.eT_TEXCOORD5_centroid);
            fxTokenKey("TEXCOORD6_centroid", EToken.eT_TEXCOORD6_centroid);
            fxTokenKey("TEXCOORD7_centroid", EToken.eT_TEXCOORD7_centroid);
            fxTokenKey("TEXCOORD8_centroid", EToken.eT_TEXCOORD8_centroid);
            fxTokenKey("TEXCOORD9_centroid", EToken.eT_TEXCOORD9_centroid);
            fxTokenKey("TEXCOORD10_centroid", EToken.eT_TEXCOORD10_centroid);
            fxTokenKey("TEXCOORD11_centroid", EToken.eT_TEXCOORD11_centroid);
            fxTokenKey("TEXCOORD12_centroid", EToken.eT_TEXCOORD12_centroid);
            fxTokenKey("TEXCOORD13_centroid", EToken.eT_TEXCOORD13_centroid);
            fxTokenKey("TEXCOORD14_centroid", EToken.eT_TEXCOORD14_centroid);
            fxTokenKey("TEXCOORD15_centroid", EToken.eT_TEXCOORD15_centroid);
            fxTokenKey("TEXCOORD16_centroid", EToken.eT_TEXCOORD16_centroid);
            fxTokenKey("TEXCOORD17_centroid", EToken.eT_TEXCOORD17_centroid);
            fxTokenKey("TEXCOORD18_centroid", EToken.eT_TEXCOORD18_centroid);
            fxTokenKey("TEXCOORD19_centroid", EToken.eT_TEXCOORD19_centroid);
            fxTokenKey("TEXCOORD20_centroid", EToken.eT_TEXCOORD20_centroid);
            fxTokenKey("TEXCOORD21_centroid", EToken.eT_TEXCOORD21_centroid);
            fxTokenKey("TEXCOORD22_centroid", EToken.eT_TEXCOORD22_centroid);
            fxTokenKey("TEXCOORD23_centroid", EToken.eT_TEXCOORD23_centroid);
            fxTokenKey("TEXCOORD24_centroid", EToken.eT_TEXCOORD24_centroid);
            fxTokenKey("TEXCOORD25_centroid", EToken.eT_TEXCOORD25_centroid);
            fxTokenKey("TEXCOORD26_centroid", EToken.eT_TEXCOORD26_centroid);
            fxTokenKey("TEXCOORD27_centroid", EToken.eT_TEXCOORD27_centroid);
            fxTokenKey("TEXCOORD28_centroid", EToken.eT_TEXCOORD28_centroid);
            fxTokenKey("TEXCOORD29_centroid", EToken.eT_TEXCOORD29_centroid);
            fxTokenKey("TEXCOORD30_centroid", EToken.eT_TEXCOORD30_centroid);
            fxTokenKey("TEXCOORD31_centroid", EToken.eT_TEXCOORD31_centroid);
            fxTokenKey("COLOR0", EToken.eT_COLOR0);

            fxTokenKey("packoffset", EToken.eT_packoffset);
            fxTokenKey("register", EToken.eT_register);
            fxTokenKey("return", EToken.eT_return);
            fxTokenKey("vsregister", EToken.eT_vsregister);
            fxTokenKey("psregister", EToken.eT_psregister);
            fxTokenKey("gsregister", EToken.eT_gsregister);
            fxTokenKey("dsregister", EToken.eT_dsregister);
            fxTokenKey("hsregister", EToken.eT_hsregister);
            fxTokenKey("csregister", EToken.eT_csregister);
            fxTokenKey("slot", EToken.eT_slot);
            fxTokenKey("vsslot", EToken.eT_vsslot);
            fxTokenKey("psslot", EToken.eT_psslot);
            fxTokenKey("dsslot", EToken.eT_gsslot);
            fxTokenKey("dsslot", EToken.eT_dsslot);
            fxTokenKey("hsslot", EToken.eT_hsslot);
            fxTokenKey("csslot", EToken.eT_csslot);
            fxTokenKey("color", EToken.eT_color);

            fxTokenKey("Buffer", EToken.eT_Buffer);
            fxTokenKey("RWBuffer", EToken.eT_RWBuffer);
            fxTokenKey("StructuredBuffer", EToken.eT_StructuredBuffer);
            fxTokenKey("RWStructuredBuffer", EToken.eT_RWStructuredBuffer);
            fxTokenKey("ByteAddressBuffer", EToken.eT_ByteAddressBuffer);
            fxTokenKey("RWByteAddressBuffer", EToken.eT_RWByteAddressBuffer);

            fxTokenKey("Position", EToken.eT_Position);
            fxTokenKey("Allways", EToken.eT_Allways);

            fxTokenKey("STANDARDSGLOBAL", EToken.eT_STANDARDSGLOBAL);

            fxTokenKey("technique", EToken.eT_technique);
            fxTokenKey("string", EToken.eT_string);
            fxTokenKey("UIName", EToken.eT_UIName);
            fxTokenKey("UIDescription", EToken.eT_UIDescription);
            fxTokenKey("UIWidget", EToken.eT_UIWidget);
            fxTokenKey("UIWidget0", EToken.eT_UIWidget0);
            fxTokenKey("UIWidget1", EToken.eT_UIWidget1);
            fxTokenKey("UIWidget2", EToken.eT_UIWidget2);
            fxTokenKey("UIWidget3", EToken.eT_UIWidget3);

            fxTokenKey("Texture", EToken.eT_Texture);
            fxTokenKey("Filter", EToken.eT_Filter);
            fxTokenKey("MinFilter", EToken.eT_MinFilter);
            fxTokenKey("MagFilter", EToken.eT_MagFilter);
            fxTokenKey("MipFilter", EToken.eT_MipFilter);
            fxTokenKey("AddressU", EToken.eT_AddressU);
            fxTokenKey("AddressV", EToken.eT_AddressV);
            fxTokenKey("AddressW", EToken.eT_AddressW);
            fxTokenKey("BorderColor", EToken.eT_BorderColor);
            fxTokenKey("AnisotropyLevel", EToken.eT_AnisotropyLevel);
            fxTokenKey("sRGBLookup", EToken.eT_sRGBLookup);
            fxTokenKey("Global", EToken.eT_Global);

            fxTokenKey("LINEAR", EToken.eT_LINEAR);
            fxTokenKey("POINT", EToken.eT_POINT);
            fxTokenKey("NONE", EToken.eT_NONE);
            fxTokenKey("ANISOTROPIC", EToken.eT_ANISOTROPIC);
            fxTokenKey("MIN_MAG_MIP_POINT", EToken.eT_MIN_MAG_MIP_POINT);
            fxTokenKey("MIN_MAG_MIP_LINEAR", EToken.eT_MIN_MAG_MIP_LINEAR);
            fxTokenKey("MIN_MAG_LINEAR_MIP_POINT", EToken.eT_MIN_MAG_LINEAR_MIP_POINT);
            fxTokenKey("COMPARISON_MIN_MAG_LINEAR_MIP_POINT", EToken.eT_COMPARISON_MIN_MAG_LINEAR_MIP_POINT);
            fxTokenKey("MINIMUM_MIN_MAG_MIP_LINEAR", EToken.eT_MINIMUM_MIN_MAG_MIP_LINEAR);
            fxTokenKey("MAXIMUM_MIN_MAG_MIP_LINEAR", EToken.eT_MAXIMUM_MIN_MAG_MIP_LINEAR);

            fxTokenKey("Clamp", EToken.eT_Clamp);
            fxTokenKey("Border", EToken.eT_Border);
            fxTokenKey("Wrap", EToken.eT_Wrap);
            fxTokenKey("Mirror", EToken.eT_Mirror);

            fxTokenKey("Script", EToken.eT_Script);

            fxTokenKey("RenderOrder", EToken.eT_RenderOrder);
            fxTokenKey("ProcessOrder", EToken.eT_ProcessOrder);
            fxTokenKey("RenderCamera", EToken.eT_RenderCamera);
            fxTokenKey("RenderType", EToken.eT_RenderType);
            fxTokenKey("RenderFilter", EToken.eT_RenderFilter);
            fxTokenKey("RenderColorTarget1", EToken.eT_RenderColorTarget1);
            fxTokenKey("RenderDepthStencilTarget", EToken.eT_RenderDepthStencilTarget);
            fxTokenKey("ClearSetColor", EToken.eT_ClearSetColor);
            fxTokenKey("ClearSetDepth", EToken.eT_ClearSetDepth);
            fxTokenKey("ClearTarget", EToken.eT_ClearTarget);
            fxTokenKey("RenderTarget_IDPool", EToken.eT_RenderTarget_IDPool);
            fxTokenKey("RenderTarget_UpdateType", EToken.eT_RenderTarget_UpdateType);
            fxTokenKey("RenderTarget_Width", EToken.eT_RenderTarget_Width);
            fxTokenKey("RenderTarget_Height", EToken.eT_RenderTarget_Height);
            fxTokenKey("GenerateMips", EToken.eT_GenerateMips);

            fxTokenKey("PreProcess", EToken.eT_PreProcess);
            fxTokenKey("PostProcess", EToken.eT_PostProcess);
            fxTokenKey("PreDraw", EToken.eT_PreDraw);

            fxTokenKey("WaterReflection", EToken.eT_WaterReflection);
            fxTokenKey("Panorama", EToken.eT_Panorama);

            fxTokenKey("WaterPlaneReflected", EToken.eT_WaterPlaneReflected);
            fxTokenKey("PlaneReflected", EToken.eT_PlaneReflected);
            fxTokenKey("Current", EToken.eT_Current);

            fxTokenKey("CurObject", EToken.eT_CurObject);
            fxTokenKey("CurScene", EToken.eT_CurScene);
            fxTokenKey("RecursiveScene", EToken.eT_RecursiveScene);
            fxTokenKey("CopyScene", EToken.eT_CopyScene);

            fxTokenKey("Refractive", EToken.eT_Refractive);
            fxTokenKey("ForceRefractionUpdate", EToken.eT_ForceRefractionUpdate);
            fxTokenKey("Heat", EToken.eT_Heat);

            fxTokenKey("DepthBuffer", EToken.eT_DepthBuffer);
            fxTokenKey("DepthBufferTemp", EToken.eT_DepthBufferTemp);
            fxTokenKey("DepthBufferOrig", EToken.eT_DepthBufferOrig);

            fxTokenKey("$ScreenSize", EToken.eT__ScreenSize);
            fxTokenKey("WaterReflect", EToken.eT_WaterReflect);
            fxTokenKey("FogColor", EToken.eT_FogColor);

            fxTokenKey("Color", EToken.eT_Color);
            fxTokenKey("Depth", EToken.eT_Depth);

            fxTokenKey("$RT_2D", EToken.eT__RT_2D);
            fxTokenKey("$RT_CM", EToken.eT__RT_CM);
            fxTokenKey("$RT_Cube", EToken.eT__RT_Cube);

            fxTokenKey("pass", EToken.eT_pass);
            fxTokenKey("CustomRE", EToken.eT_CustomRE);
            fxTokenKey("Style", EToken.eT_Style);

            fxTokenKey("VertexShader", EToken.eT_VertexShader);
            fxTokenKey("PixelShader", EToken.eT_PixelShader);
            fxTokenKey("GeometryShader", EToken.eT_GeometryShader);
            fxTokenKey("DomainShader", EToken.eT_DomainShader);
            fxTokenKey("HullShader", EToken.eT_HullShader);
            fxTokenKey("ComputeShader", EToken.eT_ComputeShader);
            fxTokenKey("ZEnable", EToken.eT_ZEnable);
            fxTokenKey("ZWriteEnable", EToken.eT_ZWriteEnable);
            fxTokenKey("CullMode", EToken.eT_CullMode);
            fxTokenKey("SrcBlend", EToken.eT_SrcBlend);
            fxTokenKey("DestBlend", EToken.eT_DestBlend);
            fxTokenKey("AlphaBlendEnable", EToken.eT_AlphaBlendEnable);
            fxTokenKey("AlphaFunc", EToken.eT_AlphaFunc);
            fxTokenKey("AlphaRef", EToken.eT_AlphaRef);
            fxTokenKey("ZFunc", EToken.eT_ZFunc);
            fxTokenKey("ColorWriteEnable", EToken.eT_ColorWriteEnable);
            fxTokenKey("IgnoreMaterialState", EToken.eT_IgnoreMaterialState);

            fxTokenKey("None", EToken.eT_None);
            fxTokenKey("Disable", EToken.eT_Disable);
            fxTokenKey("CCW", EToken.eT_CCW);
            fxTokenKey("CW", EToken.eT_CW);
            fxTokenKey("Back", EToken.eT_Back);
            fxTokenKey("Front", EToken.eT_Front);

            fxTokenKey("Never", EToken.eT_Never);
            fxTokenKey("Less", EToken.eT_Less);
            fxTokenKey("Equal", EToken.eT_Equal);
            fxTokenKey("LEqual", EToken.eT_LEqual);
            fxTokenKey("LessEqual", EToken.eT_LessEqual);
            fxTokenKey("NotEqual", EToken.eT_NotEqual);
            fxTokenKey("GEqual", EToken.eT_GEqual);
            fxTokenKey("GreaterEqual", EToken.eT_GreaterEqual);
            fxTokenKey("Greater", EToken.eT_Greater);

            fxTokenKey("Always", EToken.eT_Always);

            fxTokenKey("RED", EToken.eT_RED);
            fxTokenKey("GREEN", EToken.eT_GREEN);
            fxTokenKey("BLUE", EToken.eT_BLUE);
            fxTokenKey("ALPHA", EToken.eT_ALPHA);

            fxTokenKey("ONE", EToken.eT_ONE);
            fxTokenKey("ZERO", EToken.eT_ZERO);
            fxTokenKey("SRC_COLOR", EToken.eT_SRC_COLOR);
            fxTokenKey("SrcColor", EToken.eT_SrcColor);
            fxTokenKey("ONE_MINUS_SRC_COLOR", EToken.eT_ONE_MINUS_SRC_COLOR);
            fxTokenKey("InvSrcColor", EToken.eT_InvSrcColor);
            fxTokenKey("SRC_ALPHA", EToken.eT_SRC_ALPHA);
            fxTokenKey("SrcAlpha", EToken.eT_SrcAlpha);
            fxTokenKey("ONE_MINUS_SRC_ALPHA", EToken.eT_ONE_MINUS_SRC_ALPHA);
            fxTokenKey("InvSrcAlpha", EToken.eT_InvSrcAlpha);
            fxTokenKey("DST_ALPHA", EToken.eT_DST_ALPHA);
            fxTokenKey("DestAlpha", EToken.eT_DestAlpha);
            fxTokenKey("ONE_MINUS_DST_ALPHA", EToken.eT_ONE_MINUS_DST_ALPHA);
            fxTokenKey("InvDestAlpha", EToken.eT_InvDestAlpha);
            fxTokenKey("DST_COLOR", EToken.eT_DST_COLOR);
            fxTokenKey("DestColor", EToken.eT_DestColor);
            fxTokenKey("ONE_MINUS_DST_COLOR", EToken.eT_ONE_MINUS_DST_COLOR);
            fxTokenKey("InvDestColor", EToken.eT_InvDestColor);
            fxTokenKey("SRC_ALPHA_SATURATE", EToken.eT_SRC_ALPHA_SATURATE);

            fxTokenKey("NULL", EToken.eT_NULL);

            fxTokenKey("cbuffer", EToken.eT_cbuffer);
            fxTokenKey("PER_BATCH", EToken.eT_PER_BATCH);
            fxTokenKey("PER_INSTANCE", EToken.eT_PER_INSTANCE);
            fxTokenKey("PER_MATERIAL", EToken.eT_PER_MATERIAL);
            fxTokenKey("SKIN_DATA", EToken.eT_SKIN_DATA);
            fxTokenKey("INSTANCE_DATA", EToken.eT_INSTANCE_DATA);

            fxTokenKey("ShaderType", EToken.eT_ShaderType);
            fxTokenKey("ShaderDrawType", EToken.eT_ShaderDrawType);
            fxTokenKey("PreprType", EToken.eT_PreprType);
            fxTokenKey("Public", EToken.eT_Public);
            fxTokenKey("NoPreview", EToken.eT_NoPreview);
            fxTokenKey("LocalConstants", EToken.eT_LocalConstants);
            fxTokenKey("Cull", EToken.eT_Cull);
            fxTokenKey("SupportsAttrInstancing", EToken.eT_SupportsAttrInstancing);
            fxTokenKey("SupportsConstInstancing", EToken.eT_SupportsConstInstancing);
            fxTokenKey("SupportsDeferredShading", EToken.eT_SupportsDeferredShading);
            fxTokenKey("SupportsFullDeferredShading", EToken.eT_SupportsFullDeferredShading);
            fxTokenKey("Decal", EToken.eT_Decal);
            fxTokenKey("DecalNoDepthOffset", EToken.eT_DecalNoDepthOffset);
            fxTokenKey("NoChunkMerging", EToken.eT_NoChunkMerging);
            fxTokenKey("ForceTransPass", EToken.eT_ForceTransPass);
            fxTokenKey("AfterHDRPostProcess", EToken.eT_AfterHDRPostProcess);
            fxTokenKey("AfterPostProcess", EToken.eT_AfterPostProcess);
            fxTokenKey("ForceZpass", EToken.eT_ForceZpass);
            fxTokenKey("ForceWaterPass", EToken.eT_ForceWaterPass);
            fxTokenKey("ForceDrawLast", EToken.eT_ForceDrawLast);
            fxTokenKey("ForceDrawFirst", EToken.eT_ForceDrawFirst);
            fxTokenKey("ForceDrawAfterWater", EToken.eT_ForceDrawAfterWater);
            fxTokenKey("DepthFixup", EToken.eT_DepthFixup);
            fxTokenKey("DepthFixupReplace", EToken.eT_DepthFixupReplace);
            fxTokenKey("SingleLightPass", EToken.eT_SingleLightPass);
            fxTokenKey("HWTessellation", EToken.eT_HWTessellation);
            fxTokenKey("VertexColors", EToken.eT_VertexColors);
            fxTokenKey("WaterParticle", EToken.eT_WaterParticle);
            fxTokenKey("AlphaBlendShadows", EToken.eT_AlphaBlendShadows);
            fxTokenKey("ZPrePass", EToken.eT_ZPrePass);
            fxTokenKey("WrinkleBlending", EToken.eT_WrinkleBlending);

            fxTokenKey("VT_DetailBendingGrass", EToken.eT_VT_DetailBendingGrass);
            fxTokenKey("VT_DetailBending", EToken.eT_VT_DetailBending);
            fxTokenKey("VT_WindBending", EToken.eT_VT_WindBending);

            fxTokenKey("Light", EToken.eT_Light);
            fxTokenKey("Shadow", EToken.eT_Shadow);
            fxTokenKey("Fur", EToken.eT_Fur);
            fxTokenKey("General", EToken.eT_General);
            fxTokenKey("Terrain", EToken.eT_Terrain);
            fxTokenKey("Overlay", EToken.eT_Overlay);
            fxTokenKey("NoDraw", EToken.eT_NoDraw);
            fxTokenKey("Custom", EToken.eT_Custom);
            fxTokenKey("Sky", EToken.eT_Sky);
            fxTokenKey("OceanShore", EToken.eT_OceanShore);
            fxTokenKey("Hair", EToken.eT_Hair);
            fxTokenKey("Compute", EToken.eT_Compute);
            fxTokenKey("ForceGeneralPass", EToken.eT_ForceGeneralPass);
            fxTokenKey("EyeOverlay", EToken.eT_EyeOverlay);

            fxTokenKey("Metal", EToken.eT_Metal);
            fxTokenKey("Ice", EToken.eT_Ice);
            fxTokenKey("Water", EToken.eT_Water);
            fxTokenKey("FX", EToken.eT_FX);
            fxTokenKey("HDR", EToken.eT_HDR);
            fxTokenKey("HUD3D", EToken.eT_HUD3D);
            fxTokenKey("Glass", EToken.eT_Glass);
            fxTokenKey("Vegetation", EToken.eT_Vegetation);
            fxTokenKey("Particle", EToken.eT_Particle);
            fxTokenKey("GenerateClouds", EToken.eT_GenerateClouds);
            fxTokenKey("ScanWater", EToken.eT_ScanWater);

            fxTokenKey("NoLights", EToken.eT_NoLights);
            fxTokenKey("NoMaterialState", EToken.eT_NoMaterialState);
            fxTokenKey("PositionInvariant", EToken.eT_PositionInvariant);
            fxTokenKey("TechniqueZ", EToken.eT_TechniqueZ);
            fxTokenKey("TechniqueZPrepass", EToken.eT_TechniqueZPrepass);
            fxTokenKey("TechniqueShadowGen", EToken.eT_TechniqueShadowGen);
            fxTokenKey("TechniqueMotionBlur", EToken.eT_TechniqueMotionBlur);
            fxTokenKey("TechniqueCustomRender", EToken.eT_TechniqueCustomRender);
            fxTokenKey("TechniqueEffectLayer", EToken.eT_TechniqueEffectLayer);
            fxTokenKey("TechniqueDebug", EToken.eT_TechniqueDebug);
            fxTokenKey("TechniqueWaterRefl", EToken.eT_TechniqueWaterRefl);
            fxTokenKey("TechniqueWaterCaustic", EToken.eT_TechniqueWaterCaustic);
            fxTokenKey("TechniqueThickness", EToken.eT_TechniqueThickness);

            fxTokenKey("KeyFrameParams", EToken.eT_KeyFrameParams);
            fxTokenKey("KeyFrameRandColor", EToken.eT_KeyFrameRandColor);
            fxTokenKey("KeyFrameRandIntensity", EToken.eT_KeyFrameRandIntensity);
            fxTokenKey("KeyFrameRandSpecMult", EToken.eT_KeyFrameRandSpecMult);
            fxTokenKey("KeyFrameRandPosOffset", EToken.eT_KeyFrameRandPosOffset);
            fxTokenKey("Speed", EToken.eT_Speed);

            fxTokenKey("Beam", EToken.eT_Beam);
            fxTokenKey("LensOptics", EToken.eT_LensOptics);
            fxTokenKey("Cloud", EToken.eT_Cloud);
            fxTokenKey("Ocean", EToken.eT_Ocean);

            fxTokenKey("Model", EToken.eT_Model);
            fxTokenKey("StartRadius", EToken.eT_StartRadius);
            fxTokenKey("EndRadius", EToken.eT_EndRadius);
            fxTokenKey("StartColo", EToken.eT_StartColor);
            fxTokenKey("EndColor", EToken.eT_EndColor);
            fxTokenKey("LightStyle", EToken.eT_LightStyle);
            fxTokenKey("Length", EToken.eT_Length);

            fxTokenKey("RGBStyle", EToken.eT_RGBStyle);
            fxTokenKey("Scale", EToken.eT_Scale);
            fxTokenKey("Blind", EToken.eT_Blind);
            fxTokenKey("SizeBlindScale", EToken.eT_SizeBlindScale);
            fxTokenKey("SizeBlindBias", EToken.eT_SizeBlindBias);
            fxTokenKey("IntensBlindScale", EToken.eT_IntensBlindScale);
            fxTokenKey("IntensBlindBias", EToken.eT_IntensBlindBias);
            fxTokenKey("MinLight", EToken.eT_MinLight);
            fxTokenKey("DistFactor", EToken.eT_DistFactor);
            fxTokenKey("DistIntensityFactor", EToken.eT_DistIntensityFactor);
            fxTokenKey("FadeTime", EToken.eT_FadeTime);
            fxTokenKey("Layer", EToken.eT_Layer);
            fxTokenKey("Importance", EToken.eT_Importance);
            fxTokenKey("VisAreaScale", EToken.eT_VisAreaScale);

            fxTokenKey("Poly", EToken.eT_Poly);
            fxTokenKey("Identity", EToken.eT_Identity);
            fxTokenKey("FromObj", EToken.eT_FromObj);
            fxTokenKey("FromLight", EToken.eT_FromLight);
            fxTokenKey("Fixed", EToken.eT_Fixed);

            fxTokenKey("ParticlesFile", EToken.eT_ParticlesFile);

            fxTokenKey("Gravity", EToken.eT_Gravity);
            fxTokenKey("WindDirection", EToken.eT_WindDirection);
            fxTokenKey("WindSpeed", EToken.eT_WindSpeed);
            fxTokenKey("WaveHeight", EToken.eT_WaveHeight);
            fxTokenKey("DirectionalDependence", EToken.eT_DirectionalDependence);
            fxTokenKey("ChoppyWaveFactor", EToken.eT_ChoppyWaveFactor);
            fxTokenKey("SuppressSmallWavesFactor", EToken.eT_SuppressSmallWavesFactor);

            fxTokenKey("x", EToken.eT_x);
            fxTokenKey("y", EToken.eT_y);
            fxTokenKey("z", EToken.eT_z);
            fxTokenKey("w", EToken.eT_w);
            fxTokenKey("r", EToken.eT_r);
            fxTokenKey("g", EToken.eT_g);
            fxTokenKey("b", EToken.eT_b);
            fxTokenKey("a", EToken.eT_a);

            fxTokenKey("true", EToken.eT_true);
            fxTokenKey("false", EToken.eT_false);

            fxTokenKey("0", EToken.eT_0);
            fxTokenKey("1", EToken.eT_1);
            fxTokenKey("2", EToken.eT_2);
            fxTokenKey("3", EToken.eT_3);
            fxTokenKey("4", EToken.eT_4);
            fxTokenKey("5", EToken.eT_5);
            fxTokenKey("6", EToken.eT_6);
            fxTokenKey("7", EToken.eT_7);
            fxTokenKey("8", EToken.eT_8);
            fxTokenKey("9", EToken.eT_9);
            fxTokenKey("10", EToken.eT_10);
            fxTokenKey("11", EToken.eT_11);
            fxTokenKey("12", EToken.eT_12);
            fxTokenKey("13", EToken.eT_13);
            fxTokenKey("14", EToken.eT_14);
            fxTokenKey("15", EToken.eT_15);

            fxTokenKey("ORBIS", EToken.eT_ORBIS);
            fxTokenKey("DURANGO", EToken.eT_DURANGO);
            fxTokenKey("PCDX11", EToken.eT_PCDX11);
            fxTokenKey("PCDX12", EToken.eT_PCDX12);
            fxTokenKey("VULKAN", EToken.eT_VULKAN);

            fxTokenKey("STANDARDSGLOBAL", EToken.eT_STANDARDSGLOBAL);

            fxTokenKey("Load", EToken.eT_Load);
            fxTokenKey("Sample", EToken.eT_Sample);
            fxTokenKey("Gather", EToken.eT_Gather);
            fxTokenKey("GatherRed", EToken.eT_GatherRed);
            fxTokenKey("GatherGreen", EToken.eT_GatherGreen);
            fxTokenKey("GatherBlue", EToken.eT_GatherBlue);
            fxTokenKey("GatherAlpha", EToken.eT_GatherAlpha);

            fxTokenKey("$AutoGS_MultiRes", EToken.eT__AutoGS_MultiRes);
            fxTokenKey("Billboard", EToken.eT_Billboard);
            fxTokenKey("DebugHelper", EToken.eT_DebugHelper);

            foreach (var it in Parser.sStaticMacros)
            {
                bool bKey = false;
                uint nName = CParserBin.fxToken(it.Key, out bKey);
                if (!bKey)
                    nName = GetCRC32(it.Key);
                SMacroFX pr = it.Value;
                uint nMacros = 0U;
                uint[] Macro = new uint[64];
                if (!string.IsNullOrEmpty(pr.m_szMacro))
                {
                    (string s, int i) szBuf = (pr.m_szMacro, 0);
                    Parser.SkipCharacters(ref szBuf, " ");
                    if (szBuf.s[0] == 0)
                        break;
                    byte[] com = new byte[1024];
                    bKey = false;
                    var dwToken = CParserBin.NextToken(szBuf, com, out bKey);
                    if (!bKey)
                        dwToken = GetCRC32(com);
                    Macro[nMacros++] = dwToken;
                }
                AddMacro(nName, Macro, (int)nMacros, pr.m_nMask, m_StaticMacros);
            }
            Parser.sStaticMacros.Clear();

            if (!CParserBin.m_bShaderCacheGen)
            {
                //	#if CRY_PLATFORM_DESKTOP || CRY_PLATFORM_DURANGO
                //		if (CRenderer.ShaderTargetFlag != -1)
                //			SetupForPlatform(CRenderer.ShaderTargetFlag);
                //		else
                //	#endif
                //SetupForPlatform(SF_VULKAN);
                //SetupForPlatform(SF_D3D12);
                SetupForPlatform(G.SF_D3D11);
            }
        }

        public static void SetupForPlatform(uint nPlatform)
        {
            CleanPlatformMacros();
            uint[] nMacro = new[] { (uint)EToken.eT_1 };

            switch (nPlatform)
            {
                case G.SF_D3D11:
#if !CRY_PLATFORM_WINDOWS
                    AddMacro(CParserBin.fxToken("PCDX11", out var dummy), nMacro, 1, 0, m_StaticMacros);
#endif
                    Core.gRenDev.m_cEF.m_ShadersCache = "Shaders/Cache/D3D11/";
                    Core.gRenDev.m_cEF.m_ShadersFilter = "D3D11";
                    break;

                case G.SF_D3D12:
                    AddMacro(CParserBin.fxToken("PCDX12", out dummy), nMacro, 1, 0, m_StaticMacros);
                    Core.gRenDev.m_cEF.m_ShadersCache = "Shaders/Cache/D3D12/";
                    Core.gRenDev.m_cEF.m_ShadersFilter = "D3D12";
                    break;

                case G.SF_ORBIS:
                    AddMacro(CParserBin.fxToken("ORBIS", out dummy), nMacro, 1, 0, m_StaticMacros);
                    Core.gRenDev.m_cEF.m_ShadersCache = "Shaders/Cache/Orbis/";
                    Core.gRenDev.m_cEF.m_ShadersFilter = "Orbis";
                    break;

                case G.SF_DURANGO:
                    AddMacro(CParserBin.fxToken("DURANGO", out dummy), nMacro, 1, 0, m_StaticMacros);
                    Core.gRenDev.m_cEF.m_ShadersCache = "Shaders/Cache/Durango/";
                    Core.gRenDev.m_cEF.m_ShadersFilter = "Durango";
                    break;

                case G.SF_VULKAN:
                    AddMacro(CParserBin.fxToken("VULKAN", out dummy), nMacro, 1, 0, m_StaticMacros);
                    Core.gRenDev.m_cEF.m_ShadersCache = "Shaders/Cache/Vulkan/";
                    Core.gRenDev.m_cEF.m_ShadersFilter = "Vulkan";
                    break;

                default:
                    Debug.Assert(false, "Unknown platform.");
                    Core.gRenDev.m_cEF.m_ShadersCache = "Shaders/Cache/INVALIDPATH/";
                    Core.gRenDev.m_cEF.m_ShadersFilter = "INVALIDFILTER";
                    break;
            }

            m_nPlatform = nPlatform;

            SetupFeatureDefines();
            Core.gRenDev.m_cEF.m_Bin.InvalidateCache();
            Core.gRenDev.m_cEF.mfInitLookups();

            Core.gRenDev.m_cEF.m_pGlobalExt = null;
            Core.gRenDev.m_cEF.m_pGlobalExt = Core.gRenDev.m_cEF.mfCreateShaderGenInfo("RunTime", true);
        }

        public static void SetupFeatureDefines() => throw new NotImplementedException();
        public static string GetPlatformSpecName(string orgName)
        {
            string nmTemp = orgName;
            //if (CParserBin.m_nPlatform == G.SF_D3D11)
            //    nmTemp.add(0x200);
            //else if (CParserBin.m_nPlatform == G.SF_D3D12)
            //    nmTemp.add(0x100);
            //else if (CParserBin.m_nPlatform == G.SF_VULKAN)
            //    nmTemp.add(0x400);
            //else if (CParserBin.m_nPlatform == G.SF_ORBIS)
            //    nmTemp.add(0x600);
            //else if (CParserBin.m_nPlatform == G.SF_DURANGO)
            //    nmTemp.add(0x700);
            //else if (CParserBin.m_bEndians)
            //    nmTemp.add(0x500);

            return nmTemp;
        }

        public static string GetPlatformShaderlistName()
        {
            if (CParserBin.m_nPlatform == G.SF_D3D11)
                return "ShaderList_PC.txt";
            if (CParserBin.m_nPlatform == G.SF_D3D12)
                return "ShaderList_PC.txt";
            if (CParserBin.m_nPlatform == G.SF_DURANGO)
                return "ShaderList_Durango.txt";
            if (CParserBin.m_nPlatform == G.SF_ORBIS)
                return "ShaderList_Orbis.txt";
            if (CParserBin.m_nPlatform == G.SF_VULKAN)
                return "ShaderList_Vulkan.txt";

            throw new Exception("Unexpected Shader Platform/No platform specified");
            //return "ShaderList.txt";
        }

        public static bool PlatformSupportsConstantBuffers() { return (CParserBin.m_nPlatform & (G.SF_D3D12 | G.SF_D3D11 | G.SF_ORBIS | G.SF_DURANGO | G.SF_VULKAN)) != 0; }
        public static bool PlatformSupportsGeometryShaders() { return (CParserBin.m_nPlatform & (G.SF_D3D12 | G.SF_D3D11 | G.SF_ORBIS | G.SF_DURANGO | G.SF_VULKAN)) != 0; }
        public static bool PlatformSupportsHullShaders() { return (CParserBin.m_nPlatform & (G.SF_D3D12 | G.SF_D3D11 | G.SF_ORBIS | G.SF_DURANGO | G.SF_VULKAN)) != 0; }
        public static bool PlatformSupportsDomainShaders() { return (CParserBin.m_nPlatform & (G.SF_D3D12 | G.SF_D3D11 | G.SF_ORBIS | G.SF_DURANGO | G.SF_VULKAN)) != 0; }
        public static bool PlatformSupportsComputeShaders() { return (CParserBin.m_nPlatform & (G.SF_D3D12 | G.SF_D3D11 | G.SF_ORBIS | G.SF_DURANGO | G.SF_VULKAN)) != 0; }
        public static bool PlatformIsConsole() { return (CParserBin.m_nPlatform & (G.SF_ORBIS | G.SF_DURANGO)) != 0; }

        public static bool m_bEditable;
        public static uint m_nPlatform;
        public static bool m_bEndians;
        public static bool m_bParseFX;
        public static bool m_bShaderCacheGen;
    }

    //string fxFillPr(string[] buf, string dst);
}
