using System;
using System.Collections.Generic;
using FXShaderToken = System.Collections.Generic.List<CryShader.Shaders.STokenD>;

namespace CryShader.Shaders
{
    //#define FX_CACHE_VER     1.0
    //#define FX_SER_CACHE_VER 1.2    // Shader serialization version (FX_CACHE_VER + FX_SER_CACHE_VER)

    //// Maximum 1 digit here
    //// The version determines the parse logic in the shader cache gen, these values cannot overlap
    //#define SHADER_LIST_VER      2
    //#define SHADER_SERIALISE_VER (SHADER_LIST_VER + 1)

    ////#define SHADER_NO_SOURCES 1 // If this defined all binary shaders (.fxb) should be located in Game folder (not user)

    //#define SHADERS_SERIALIZING 1 // Enables shaders serializing (Export/Import) to/from .fxb files

    //#define CB_PER_DRAW         eConstantBufferShaderSlot_PerDraw           // Non-Scene-Pass only
    //#define CB_PER_MATERIAL     eConstantBufferShaderSlot_PerMaterial       // Scene-Pass only
    //#define CB_NUM              2

    ////====================================================================
    //// Fixed Per-Material constants
    //// Needs to match shader registers in FXConstantDefs

    //#define NUM_PM_CONSTANTS             (10 + EFTT_MAX * 2)
    //#define FIRST_REG_PM                 (0)

    //#define REG_PM_CHANNELS_SB           (0)                  // Scale-Bias for each texture slot
    //#define REG_PM_DIFFUSE_COL           (0 + EFTT_MAX * 2)
    //#define REG_PM_SPECULAR_COL          (1 + EFTT_MAX * 2)
    //#define REG_PM_EMISSIVE_COL          (2 + EFTT_MAX * 2)
    //#define REG_PM_TCM_MATRIX            (3 + EFTT_MAX * 2)
    //#define REG_PM_DEFORM_WAVE           (7 + EFTT_MAX * 2)
    //#define REG_PM_DETAILTILING_ALPHAREF (8 + EFTT_MAX * 2)
    //#define REG_PM_SILPOM_DETAIL_PARAMS  (9 + EFTT_MAX * 2)

    public enum eCompareFunc
    {
        eCF_Disable,
        eCF_Never,
        eCF_Less,
        eCF_Equal,
        eCF_LEqual,
        eCF_Greater,
        eCF_NotEqual,
        eCF_GEqual,
        eCF_Always
    }

    public struct SPair
    {
        public string m_szMacroName;
        public string m_szMacro;
        public uint m_nMask;
    }

    //const bool GEOMETRYSHADER_SUPPORT = true; //false

    public class SFXSampler
    {
        public string m_Name; // Parameter name
        public List<uint> m_dwName;
        public uint m_nFlags;
        public short m_nArray;      // Number of samplers
        public string m_Annotations; // Additional parameters (between <>)
        public string m_Semantic;    // Parameter semantic type (after ':')
        public string m_Values;      // Parameter values (after '=')
        public byte m_eType;       // ESamplerType
        public short[] m_nRegister = new short[(int)EHWShaderClass.eHWSC_Num];
        public SamplerStateHandle m_nTexState = null;

        public SFXSampler()
        {
            for (var i = 0; i < (int)EHWShaderClass.eHWSC_Num; i++)
                m_nRegister[i] = 10000;
        }

        public uint GetFlags() => m_nFlags;
        //public void PostLoad(CParserBin Parser, SParserFrame Name, SParserFrame Annotations, SParserFrame Values, SParserFrame Assign);
        //public bool Export(SShaderSerializeContext SC);
        //public bool Import(SShaderSerializeContext SC, SSFXSampler pPR);
        //public uint Size()
        //{
        //    uint nSize = sizeof(SFXSampler);
        //    //nSize += m_Name.capacity();
        //    nSize += sizeofVector(m_dwName);
        //    //nSize += m_Values.capacity();
        //    return nSize;
        //}
        //public bool operator ==(SFXSampler m)
        //{
        //    if (m_Name == m.m_Name && m_Annotations == m.m_Annotations && m_Semantic == m.m_Semantic && m_Values == m.m_Values &&
        //    m_nArray == m.m_nArray && m_nFlags == m.m_nFlags && m_nRegister[0] == m.m_nRegister[0] && m_nRegister[1] == m.m_nRegister[1] &&
        //    m_eType == m.m_eType && m_nTexState == m.m_nTexState)
        //        return true;
        //    return false;
        //}
        //public void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    //pSizer->AddObject( m_Name );
        //    //pSizer->AddObject( m_Values );
        //    pSizer->AddObject(m_dwName);
        //}
    }

    public class SFXTexture
    {
        public string m_Name; // Texture name
        public List<uint> m_dwName;
        public uint m_nFlags;
        public uint m_nTexFlags;
        public short m_nArray;      // Number of textures
        public string m_Annotations; // Additional parameters (between <>)
        public string m_Semantic;    // Parameter semantic type (after ':')
        public string m_Values;      // Parameter values (after '=')
        public string m_szTexture;   // Texture source name
        public SHRenderTarget m_pTarget = null;
        public string m_szUIName;    // UI name
        public string m_szUIDesc;    // UI description
        public bool m_bSRGBLookup; // Lookup
        public byte m_eType;       // ETextureType
        public byte m_Type;        // Data type (float, float4, etc)
        public short[] m_nRegister = new short[(int)EHWShaderClass.eHWSC_Num];

        public SFXTexture()
        {
            for (var i = 0; i < (int)EHWShaderClass.eHWSC_Num; i++)
                m_nRegister[i] = 10000;
        }

        public uint GetFlags() => m_nFlags;
        public uint GetTexFlags() => m_nTexFlags;
        //public void PostLoad(CParserBin Parser, SParserFrame Name, SParserFrame Annotations, SParserFrame Values, SParserFrame Assign);
        //public bool Export(SShaderSerializeContext SC);
        //public bool Import(SShaderSerializeContext SC, SSFXTexture pPR);
        //public uint Size()
        //{
        //    uint nSize = sizeof(SFXTexture);
        //    //nSize += m_Name.capacity();
        //    nSize += sizeofVector(m_dwName);
        //    //nSize += m_Values.capacity();
        //    return nSize;
        //}
        //public bool operator ==(SFXTexture m)
        //{
        //    if (m_Name == m.m_Name && m_Annotations == m.m_Annotations && m_Semantic == m.m_Semantic && m_Values == m.m_Values &&
        //        m_nArray == m.m_nArray && m_nFlags == m.m_nFlags && m_nRegister[0] == m.m_nRegister[0] && m_nRegister[1] == m.m_nRegister[1] &&
        //        m_eType == m.m_eType && m_bSRGBLookup == m.m_bSRGBLookup && m_szTexture == m.m_szTexture)
        //        return true;
        //    return false;
        //}
        //public void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    //pSizer->AddObject( m_Name );
        //    //pSizer->AddObject( m_Values );
        //    pSizer->AddObject(m_dwName);
        //}
    }

    // In Matrix 3x4: m_nParams = 3, m_nComps = 4
    public class SFXParam
    {
        public string m_Name; // Parameter name
        public List<uint> m_dwName;
        public uint m_nFlags;
        public short m_nParameters; // Number of parameters
        public short m_nComps;      // Number of components in single parameter
        public string m_Annotations; // Additional parameters (between <>)
        public string m_Semantic;    // Parameter semantic type (after ':')
        public string m_Values;      // Parameter values (after '=')
        public byte m_eType;       // EParamType
        public byte m_nCB = byte.MaxValue;
        public short[] m_nRegister = new short[(int)EHWShaderClass.eHWSC_Num];
        public ulong m_mask;

        public SFXParam()
        {
            m_nRegister[0] = 10000;
            m_nRegister[1] = 10000;
            m_nRegister[2] = 10000;
            m_nRegister[3] = 10000;
            m_nRegister[4] = 10000;
            m_nRegister[5] = 10000;
        }

        //public uint GetComponent(EHWShaderClass eSHClass);
        //public void GetParamComp(uint32 nOffset, CryFixedStringT<128> param);
        public uint GetParamFlags() => m_nFlags;
        //public void GetCompName(uint nId, CryFixedStringT<128> name);
        //public string GetValueForName(string szName, EParamType eType);
        //public void PostLoad(CParserBin Parser, SParserFrame Name, SParserFrame Annotations, SParserFrame Values, SParserFrame Assign);
        //public void PostLoad();
        //public bool Export(SShaderSerializeContext SC);
        //public bool Import(SShaderSerializeContext SC, SSFXParam pPR);
        //public uint Size()
        //{
        //    uint nSize = sizeof(SFXParam);
        //    //nSize += m_Name.capacity();
        //    nSize += sizeofVector(m_dwName);
        //    //nSize += m_Values.capacity();
        //    return nSize;
        //}
        //public bool operator ==(SFXParam m)
        //{
        //    if (m_Name == m.m_Name && m_Annotations == m.m_Annotations && m_Semantic == m.m_Semantic && m_Values == m.m_Values &&
        //        m_nParameters == m.m_nParameters && m_nComps == m.m_nComps && m_nFlags == m.m_nFlags && m_nRegister[0] == m.m_nRegister[0] && m_nRegister[1] == m.m_nRegister[1] &&
        //        m_eType == m.m_eType)
        //        return true;
        //    return false;
        //}
        //public void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    //pSizer->AddObject( m_Name );
        //    //pSizer->AddObject( m_Values );
        //    pSizer->AddObject(m_dwName);
        //}
    }

    public struct STokenD
    {
        //public List<int> Offsets;
        public uint Token;
        public string SToken;
        //public uint Size() { return sizeof(STokenD) /*+ sizeofVector(Offsets)*/ + SToken.capacity(); }
        //public void GetMemoryUsage(ICrySizer pSizer) { pSizer->AddObject(SToken); }
    }

    public struct SFXStruct
    {
        public string m_Name;
        public string m_Struct;
    }

    public enum ETexFilter
    {
        eTEXF_None,
        eTEXF_Point,
        eTEXF_Linear,
        eTEXF_Anisotropic,
    }

    //=============================================================================
    // Vertex programms / Vertex shaders (VP/VS)

    //=====================================================================

    //static float* sfparam(Vec3 param)
    //{
    //    static float sparam[4];
    //    sparam[0] = param.x;
    //    sparam[1] = param.y;
    //    sparam[2] = param.z;
    //    sparam[3] = 1.0f;
    //    return &sparam[0];
    //}

    //static float* sfparam(float param)
    //{
    //    static float sparam[4];
    //    sparam[0] = param;
    //    sparam[1] = 0;
    //    sparam[2] = 0;
    //    sparam[3] = 1.0f;
    //    return &sparam[0];
    //}

    //static float* sfparam(float param0, float param1, float param2, float param3)
    //{
    //    static float sparam[4];
    //    sparam[0] = param0;
    //    sparam[1] = param1;
    //    sparam[2] = param2;
    //    sparam[3] = param3;
    //    return &sparam[0];
    //}

    //string sGetFuncName(string pFunc)
    //{
    //    static char func[128];
    //    const char* b = pFunc;
    //    if (*b == '[')
    //    {
    //        const char* s = strchr(b, ']');
    //        if (s) b = s + 1;
    //        while (*b <= 0x20) b++;
    //    }
    //    while (*b > 0x20) b++;
    //    while (*b <= 0x20) b++;
    //    int n = 0;
    //    while (*b > 0x20 && *b != '(') func[n++] = *b++;
    //    func[n] = 0;
    //    return func;
    //}

    public enum ERenderOrder
    {
        eRO_PreProcess,
        eRO_PostProcess,
        eRO_PreDraw,
        eRO_Managed
    }

    public enum ERTUpdate
    {
        eRTUpdate_Unknown,
        eRTUpdate_Always,
        eRTUpdate_WaterReflect
    }

    //struct SHRenderTarget //: IRenderTarget
    //{
    //    ERenderOrder m_eOrder = ERenderOrder.eRO_PreProcess;
    //    int m_nProcessFlags; // FSPR_ flags
    //    string m_TargetName;
    //    int m_nWidth = 256;
    //    int m_nHeight = 256;
    //    ETEX_Format m_eTF = eTF_R8G8B8A8;
    //    int m_nIDInPool = -1;
    //    ERTUpdate m_eUpdateType = eRTUpdate_Unknown;
    //    CTexture m_pTarget;
    //    bool m_bTempDepth = true;
    //    ColorF m_ClearColor = Col_Black;
    //    float m_fClearDepth = 1.f;
    //    uint m_nFlags;
    //    uint m_nFilterFlags = 0xffffffff;
    //    int m_refSamplerID = -1;

    //    void Release() { }
    //    void AddRef() { }

    //    //SEnvTexture GetEnv2D();
    //    //SEnvTexture GetEnvCM();
    //    //void GetMemoryUsage(ICrySizer pSizer);
    //}

    //=============================================================================
    // Hardware shaders

    partial class G
    {
        public const int SHADER_BIND_TEXTURE = 0x2000;
        public const int SHADER_BIND_SAMPLER = 0x4000;
    }

    //=============================================================================

    public struct SShaderCacheHeaderItem
    {
        public byte m_nVertexFormat;
        public byte m_Class;
        public byte m_nInstBinds;
        public byte m_StreamMask_Stream;
        public uint m_CRC32;
        public ushort m_StreamMask_Decl;
        public short m_nInstructions;
    }

    //#define MAX_VAR_NAME 512
    public struct SShaderCacheHeaderItemVar
    {
        public int m_Reg;
        public short m_nCount;
        public string m_Name; //[MAX_VAR_NAME]
    }

    public struct SCompressedData
    {
        public byte[] m_pCompressedShader;
        public uint m_nSizeCompressedShader;
        public uint m_nSizeDecompressedShader;

        //int Size()
        //{
        //    int nSize = sizeof(SCompressedData);
        //    if (m_pCompressedShader)
        //        nSize += m_nSizeCompressedShader;
        //    return nSize;
        //}
        //void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    //pSizer->AddObject(this, sizeof(SCompressedData));
        //    if (m_pCompressedShader)
        //        pSizer->AddObject(m_pCompressedShader, m_nSizeCompressedShader);
        //}
    }

    public enum cacheSource : byte
    {
        readonly_ = 0,       // Shader cache from readonly paks
        user = 1,           // Writeable shader cache
    }

    public struct SOptimiseStats
    {
        public int nEntries;
        public int nUniqueEntries;
        public int nSizeUncompressed;
        public int nSizeCompressed;
        public int nDirDataSize;
    }

    //====================================================================
    // HWShader run-time flags
    // Note:we are limited to a maximum of 64, check HWSR_MAX before adding

    public enum EHWSRMaskBit
    {
        HWSR_FOG = 0,

        HWSR_ALPHATEST,
        HWSR_ALPHABLEND,

        HWSR_MSAA_QUALITY,
        HWSR_MSAA_QUALITY1,
        HWSR_MSAA_SAMPLEFREQ_PASS,

        HWSR_SECONDARY_VIEW,

        HWSR_VERTEX_VELOCITY,
        HWSR_SKELETON_SSD,
        HWSR_SKELETON_SSD_LINEAR,
        HWSR_COMPUTE_SKINNING,

        HWSR_OBJ_IDENTITY,
        HWSR_NEAREST,
        HWSR_DISSOLVE,
        HWSR_NO_TESSELLATION,

        HWSR_QUALITY,
        HWSR_QUALITY1,

        HWSR_SAMPLE0,
        HWSR_SAMPLE1,
        HWSR_SAMPLE2,
        HWSR_SAMPLE3,
        HWSR_SAMPLE4,
        HWSR_SAMPLE5,
        HWSR_SAMPLE6,

        HWSR_DEBUG0,
        HWSR_DEBUG1,
        HWSR_DEBUG2,
        HWSR_DEBUG3,

        HWSR_CUBEMAP0,

        HWSR_DECAL_TEXGEN_2D,

        HWSR_SHADOW_DEPTH_OUTPUT_LINEAR,
        HWSR_HW_PCF_COMPARE,
        HWSR_SHADOW_JITTERING,
        HWSR_POINT_LIGHT,
        HWSR_LIGHT_TEX_PROJ,

        HWSR_BLEND_WITH_TERRAIN_COLOR,
        HWSR_AMBIENT_OCCLUSION,

        HWSR_PARTICLE_SHADOW,
        HWSR_SOFT_PARTICLE,
        HWSR_OCEAN_PARTICLE,
        HWSR_ANIM_BLEND,
        HWSR_ENVIRONMENT_CUBEMAP,
        HWSR_MOTION_BLUR,

        HWSR_SPRITE,

        HWSR_LIGHTVOLUME0,
        HWSR_LIGHTVOLUME1,

        HWSR_TILED_SHADING,

        HWSR_VOLUMETRIC_FOG,

        HWSR_REVERSE_DEPTH,

        HWSR_PROJECTION_MULTI_RES,
        HWSR_PROJECTION_LENS_MATCHED,
        HWSR_MAX
    }

    partial class G
    {
        ulong[] g_HWSR_MaskBit = new ulong[(int)EHWSRMaskBit.HWSR_MAX];

        // HWShader global flags (m_Flags)
        const uint HWSG_SUPPORTS_LIGHTING = 0x20;
        const uint HWSG_SUPPORTS_MULTILIGHTS = 0x40;
        const uint HWSG_SUPPORTS_MODIF = 0x80;
        const uint HWSG_SUPPORTS_VMODIF = 0x100;
        const uint HWSG_WASGENERATED = 0x200;
        const uint HWSG_NOSPECULAR = 0x400;
        const uint HWSG_SYNC = 0x800;
        const uint HWSG_CACHE_USER = 0x1000;
        //const uint HWSG_AUTOENUMTC      =0x1000;
        const uint HWSG_UNIFIEDPOS = 0x2000;
        const uint HWSG_DEFAULTPOS = 0x4000;
        const uint HWSG_PROJECTED = 0x8000;
        const uint HWSG_NOISE = 0x10000;
        const uint HWSG_FP_EMULATION = 0x40000;
        const uint HWSG_GS_MULTIRES = 0x80000;

        // HWShader per-instance Modificator flags (SHWSInstance::m_MDMask)
        // Vertex shader specific

        // Texture projected flags
        const uint HWMD_TEXCOORD_PROJ = 0x1;
        // Texture transform flag
        const uint HWMD_TEXCOORD_MATRIX = 0x100;
        // Object linear texgen flag
        const uint HWMD_TEXCOORD_GEN_OBJECT_LINEAR = 0x1000;

        const uint HWMD_TEXCOORD_FLAG_MASK = 0xfffff000 | 0xf00;

        // HWShader per-instance vertex modificator flags (SHWSInstance::m_MDVMask)
        // Texture projected flags (4 bits)
        const int HWMDV_TYPE = 0;

        // HWShader input flags (passed via mfSet function)
        const int HWSF_SETPOINTERSFORSHADER = 1;
        const int HWSF_SETPOINTERSFORPASS = 2;
        const int HWSF_PRECACHE = 4;
        const int HWSF_SETTEXTURES = 8;
        const int HWSF_FAKE = 0x10;

        const int HWSF_INSTANCED = 0x20;
        const int HWSF_NEXT = 0x100;
        const int HWSF_PRECACHE_INST = 0x200;
        const int HWSF_STORECOMBINATION = 0x400;
        const int HWSF_STOREDATA = 0x800;
    }

    struct SDiskShaderCache
    {
        //        struct recreateUserCacheTag { }

        //        SDiskShaderCache(string name, cacheSource cacheType);
        //        SDiskShaderCache(recreateUserCacheTag, string name, uint CRC32, float cacheVer);

        //        CResFile m_pRes = null;

        //        Tuple<byte[], uint> DecompressResource(CResFileOpenScope scope, int offset, int size);

        //        //void GetMemoryUsage(ICrySizer pSizer);
        //        cacheSource GetType() => cacheType;

        //#if CRY_PLATFORM_DESKTOP
        //        bool mfOptimiseCacheFile(SOptimiseStats Stats);
        //#endif

        //        private cacheSource cacheType = cacheSource.readonly_;

        //        private bool OpenCacheFileImpl(cacheSource cacheType, CResFile pRF);
        //        private bool OpenCacheFile(string szName, cacheSource src);
    }

    struct SDeviceShaderEntry
    {
        //        SShaderCacheHeaderItem header;
        //        List<SCGBind> bindVars;
        //        byte[] m_pVertexShaderBinary;
        //        int m_VertexShaderBinarySize;
        //#if CRY_RENDERER_VULKAN
        //        List<SVertexInputStream>  m_VSInputStreams;
        //#endif
        //        SD3DShader shader;
        //        operator bool () noexcept { return !!shader; }
        //        void GetMemoryUsage(ICrySizer pSizer)
        //        {
        //            pSizer->AddObject(bindVars.data(), bindVars.size() * sizeof(SCGBind));
        //            pSizer->AddObject(m_pVertexShaderBinary.get(), m_VertexShaderBinarySize * sizeof(byte));
        //#if CRY_RENDERER_VULKAN
        //        	  pSizer->AddObject(m_VSInputStreams.data(), m_VSInputStreams.size() * sizeof(SVertexInputStream));
        //#endif
        //            pSizer->AddObject(shader.get());
        //        }
    }

    public struct SHWShaderCache //: CBaseResource
    {
        //private string m_Name;

        ////using deviceShaderCacheKey = uint;
        //// Value might be an entry or a duplicate, in which case it is a pointer to duplicated entry.
        ////using deviceShaderCacheValue = CryVariant<const SDeviceShaderEntry*, SDeviceShaderEntry>;
        ////using shaderCache = std::unordered_map<deviceShaderCacheKey, deviceShaderCacheValue>;

        //shaderCache m_shaders;
        //std::unique_ptr<SDiskShaderCache> m_pDiskShaderCache[2];

        //SHWShaderCache(string name) noexcept : m_Name(name) { }

        //SDiskShaderCache AcquireDiskCache(cacheSource src)
        //{
        //auto & cache = m_pDiskShaderCache[static_cast<int>(src)];
        //if (cache)
        //return cache.get();

        //return (cache = stl::make_unique<SDiskShaderCache>(m_Name.c_str(), src)).get();
        //}

        //const string& GetName() const { return m_Name; }
        //virtual void GetMemoryUsage(ICrySizer* pSizer) const override;
        //void Reset()
        //{
        //m_shaders.clear();
        //m_pDiskShaderCache[0] = nullptr;
        //m_pDiskShaderCache[1] = nullptr;
        //}
    }

    public class CHWShader //: public CBaseResource
    {
        //public EHWShaderClass m_eSHClass = eHWSC_Vertex;
        //public EHWSProfile m_eHWProfile;

        public static CHWShader s_pCurHWVS;
        public static string s_GS_MultiRes_NV;

        public string m_Name;
        public string m_NameSourceFX;
        public string m_EntryFunc;
        public ulong m_nMaskAnd_RT = ulong.MaxValue;
        public ulong m_nMaskOr_RT;
        public ulong m_nMaskGenShader; // Masked/Optimised m_nMaskGenFX for this specific HW shader
        public ulong m_nMaskGenFX;     // FX Shader should be parsed with this flags
        public ulong m_nMaskSetFX;     // AffectMask GL for parser tree

        public uint m_nPreprocessFlags;
        public int m_nFrame;
        public int m_nFrameLoad;
        public uint m_Flags;
        public uint m_CRC32;
        public uint m_dwShaderType;

        protected string m_CachedTokens;
        protected SHWShaderCache m_pCache;

        //public void InvalidateCache(cacheSource src) => m_pCache.m_pDiskShaderCache[(int)src] = null;
        //public void InvalidateCaches() => m_pCache.Reset();
        //public SDiskShaderCache QueryDiskCache(cacheSource src) => (m_pCache.m_pDiskShaderCache[(int)src] != null && m_pCache.m_pDiskShaderCache[(int)src].m_pRes != null) ? m_pCache.m_pDiskShaderCache[(int)src].get() : null;
        //public SDiskShaderCache AcquireDiskCache(cacheSource src) => m_pCache.AcquireDiskCache(src);
        //public shaderCache GetDevCache() => m_pCache.m_shaders;

        ////public EHWSProfile mfGetHWProfile(uint32 nFlags);

        //public static string mfGetClassName(EHWShaderClass eClass);
        //public static string mfGetCacheClassName(EHWShaderClass eClass);

        //public static CHWShader mfForName(string name, string nameSource, uint CRC32, string szEntryFunc, EHWShaderClass eClass, uint[] SHData, FXShaderToken Table, uint dwType, CShader pFX, ulong nMaskGen = 0, ulong nMaskGenFX = 0);

        //public static void mfReloadScript(string szPath, string szName, int nFlags, ulong nMaskGen);
        //public static void mfFlushPendedShadersWait(int nMaxAllowed);
        public string GetName() => m_Name;

        //public abstract int Size();
        //public abstract void GetMemoryUsage(ICrySizer Sizer);
        //public abstract void mfReset();

        //public abstract bool mfAddEmptyCombination(ulong nRT, ulong nGL, uint nLT, SCacheCombination cmbSaved);
        //public abstract bool mfStoreEmptyCombination(SEmptyCombination Comb);
        //public virtual string mfGetCurScript() => null;
        //public abstract string mfGetEntryName();
        //public abstract void mfUpdatePreprocessFlags(SShaderTechnique pTech);
        //public abstract bool mfFlushCacheFile();

        //// Used to precache shader combination during shader cache generation.
        //public abstract bool PrecacheShader(CShader pSH, SShaderCombIdent cacheIdent, uint nFlags);

        //public abstract bool Export(CShader pSH, SShaderSerializeContext SC);
        //public static CHWShader* Import(SShaderSerializeContext SC, int nOffs, uint CRC32, CShader pSH);

        //// Vertex shader specific functions
        //public abstract InputLayoutHandle mfVertexFormat(bool bUseTangents, bool bUseLM, bool bUseHWSkin);

        //public abstract string mfGetActivatedCombinations(bool bForLevel);

        //public static string mfProfileString(EHWShaderClass eClass);
        //public static string mfClassString(EHWShaderClass eClass);
        //public static EHWShaderClass mfStringProfile(string profile);
        //public static EHWShaderClass mfStringClass(string szClass);
        //public static void mfGenName(ulong GLMask, ulong RTMask, uint LightMask, uint MDMask, uint MDVMask, ulong PSS, EHWShaderClass eClass, string dstname, int nSize, byte bType);
        //public static void mfGenMasksFromName(string srcName, ulong GLMask, ulong RTMask, uint LightMask, uint MDMask, uint MDVMask, ulong PSS, EHWShaderClass eClass, int nSize, byte bType);

        //public static void mfLazyUnload();
        //public static void mfCleanupCache();

        //public static string GetCurrentShaderCombinations(bool bForLevel);

        //public static byte[] mfIgnoreRemapsFromCache(int nRemaps, byte[] pP);
        //public static byte[] mfIgnoreBindsFromCache(int nParams, byte[] pP);

        //public static void mfValidateDirEntries(CResFile pRF);

        // Import/Export
        //public static bool ImportSamplers(SShaderSerializeContext SC, SCHWShader pSHW, byte[] pData, List<STexSamplerRT> Samplers);
        //public static bool ImportParams(SShaderSerializeContext SC, SCHWShader pSHW, byte[] pData, List<SFXParam> Params);
    }

    //void SortLightTypes(int Types[4], int nCount)
    //{
    //    switch (nCount)
    //    {
    //        case 2:
    //            if (Types[0] > Types[1])
    //                Exchange(Types[0], Types[1]);
    //            break;
    //        case 3:
    //            if (Types[0] > Types[1])
    //                Exchange(Types[0], Types[1]);
    //            if (Types[0] > Types[2])
    //                Exchange(Types[0], Types[2]);
    //            if (Types[1] > Types[2])
    //                Exchange(Types[1], Types[2]);
    //            break;
    //        case 4:
    //            {
    //                for (int i = 0; i < 4; i++)
    //                {
    //                    for (int j = i; j < 4; j++)
    //                    {
    //                        if (Types[i] > Types[j])
    //                            Exchange(Types[i], Types[j]);
    //                    }
    //                }
    //            }
    //            break;
    //    }
    //}

    //=========================================================================
    // Dynamic lights evaluating via shaders

    //public enum ELightStyle
    //{
    //    eLS_Intensity,
    //    eLS_RGB,
    //}

    //public enum ELightMoveType
    //{
    //    eLMT_Wave,
    //    eLMT_Patch,
    //}

    //public struct SLightMove
    //{
    //    ELightMoveType m_eLMType;
    //    SWaveForm m_Wave;
    //    Vec3 m_Dir;
    //    float m_fSpeed;

    //    //int Size()
    //    //{
    //    //    int nSize = sizeof(SLightMove);
    //    //    return nSize;
    //    //}
    //}

    //struct SLightStyleKeyFrame
    //{
    //    ColorF cColor;     // xyz: color, w: spec mult
    //    Vec3 vPosOffset; // position offset

    //    SLightStyleKeyFrame()
    //    {
    //        cColor = ColorF(Col_Black);
    //        vPosOffset = Vec3(ZERO);
    //    }

    //    void GetMemoryUsage(ICrySizer pSizer)
    //    {
    //        pSizer->AddObject(this, sizeof(*this));
    //    }
    //}

    public class CLightStyle
    {
        //static CLightStyle[] s_LStyles;
        //SLightStyleKeyFrame[] m_Map;

        //ColorF m_Color = Col_White;      // xyz: color, w: spec mult
        //Vec3 m_vPosOffset = Vec3.ZERO; // position offset

        //float m_TimeIncr = 60.0f;
        //float m_LastTime;

        //uint8 m_bRandColor     : 1;
        //uint8 m_bRandIntensity : 1;
        //uint8 m_bRandPosOffset : 1;
        //uint8 m_bRandSpecMult  : 1;

        //int Size()
        //{
        //    int nSize = sizeof(CLightStyle);
        //    nSize += m_Map.GetMemoryUsage();
        //    return nSize;
        //}
        //void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    pSizer->Add(*this);
        //    pSizer->AddObject(m_Map);
        //}
        //static CLightStyle mfGetStyle(uint nStyle, float fTime)
        //{
        //    if (nStyle >= s_LStyles.Num() || !s_LStyles[nStyle])
        //        return null;
        //    s_LStyles[nStyle]->mfUpdate(fTime);
        //    return s_LStyles[nStyle];
        //}
        //void mfUpdate(float fTime);
    }

    //=========================================================================
    // HW Shader Layer

    //#define SHPF_AMBIENT             0x100
    //#define SHPF_HASLM               0x200
    //#define SHPF_SHADOW              0x400
    //#define SHPF_RADIOSITY           0x800
    //#define SHPF_ALLOW_SPECANTIALIAS 0x1000
    //#define SHPF_BUMP                0x2000
    //#define SHPF_NOMATSTATE          0x4000
    //#define SHPF_FORCEZFUNC          0x8000

    //    // Shader pass definition for HW shaders
    //    struct SShaderPass
    //    {
    //        uint32 m_RenderState;     // Render state flags
    //        signed char m_eCull;
    //        uint8 m_AlphaRef;

    //        uint16 m_PassFlags;         // Different usefull Pass flags (SHPF_)

    //        CHWShader* m_VShader;        // Pointer to the vertex shader for the current pass
    //        CHWShader* m_PShader;        // Pointer to fragment shader
    //        CHWShader* m_GShader;        // Pointer to the geometry shader for the current pass
    //        CHWShader* m_DShader;        // Pointer to the domain shader for the current pass
    //        CHWShader* m_HShader;        // Pointer to the hull shader for the current pass
    //        CHWShader* m_CShader;        // Pointer to the compute shader for the current pass
    //        SShaderPass();

    //        int Size()
    //        {
    //            int nSize = sizeof(SShaderPass);
    //            return nSize;
    //        }

    //        void GetMemoryUsage(ICrySizer* pSizer) const
    //    	{

    //        pSizer->AddObject(m_VShader);
    //        pSizer->AddObject(m_PShader);
    //        pSizer->AddObject(m_GShader);
    //        pSizer->AddObject(m_HShader);
    //        pSizer->AddObject(m_DShader);
    //        pSizer->AddObject(m_CShader);
    //    }
    //    void mfFree()
    //    {
    //        SAFE_RELEASE(m_VShader);
    //        SAFE_RELEASE(m_PShader);
    //        SAFE_RELEASE(m_GShader);
    //        SAFE_RELEASE(m_HShader);
    //        SAFE_RELEASE(m_DShader);
    //        SAFE_RELEASE(m_CShader);
    //    }

    //    void AddRefsToShaders()
    //    {
    //        if (m_VShader)
    //            m_VShader->AddRef();
    //        if (m_PShader)
    //            m_PShader->AddRef();
    //        if (m_GShader)
    //            m_GShader->AddRef();
    //        if (m_DShader)
    //            m_DShader->AddRef();
    //        if (m_HShader)
    //            m_HShader->AddRef();
    //        if (m_CShader)
    //            m_CShader->AddRef();
    //    }

    //    private:
    //	SShaderPass& operator=(const SShaderPass& sl);
    //};

    ////===================================================================================
    //// Hardware Stage for HW only Shaders

    //#define FHF_FIRSTLIGHT          8
    //#define FHF_FORANIM             0x10
    //#define FHF_TERRAIN             0x20
    //#define FHF_NOMERGE             0x40
    //#define FHF_DETAILPASS          0x80
    //#define FHF_LIGHTPASS           0x100
    //#define FHF_FOGPASS             0x200
    //#define FHF_PUBLIC              0x400
    //#define FHF_NOLIGHTS            0x800
    //#define FHF_POSITION_INVARIANT  0x1000
    //#define FHF_TRANSPARENT         0x40000
    //#define FHF_WASZWRITE           0x80000
    //#define FHF_USE_GEOMETRY_SHADER 0x100000
    //#define FHF_USE_HULL_SHADER     0x200000
    //#define FHF_USE_DOMAIN_SHADER   0x400000
    //#define FHF_RE_LENSOPTICS       0x1000000

    //struct SShaderTechnique
    //{
    //    CShader* m_shader;    // Shader owner of this technique.
    //    CCryNameR m_NameStr;
    //    CCryNameTSCRC m_NameCRC;
    //    TArray<SShaderPass> m_Passes;    // General passes
    //    int m_Flags;     // Different flags (FHF_)
    //    uint32 m_nPreprocessFlags;
    //    int8 m_nTechnique[TTYPE_MAX]; // Next technique in sequence
    //    TArray<CRenderElement*> m_REs;                   // List of all render elements registered in the shader
    //    TArray<SHRenderTarget*> m_RTargets;
    //    float m_fProfileTime;

    //    int Size()
    //    {
    //        uint32 i;
    //        int nSize = sizeof(SShaderTechnique);
    //        for (i = 0; i < m_Passes.Num(); i++)
    //        {
    //            nSize += m_Passes[i].Size();
    //        }
    //        nSize += m_RTargets.GetMemoryUsage();
    //        return nSize;
    //    }

    //    void GetMemoryUsage(ICrySizer* pSizer) const
    //	{

    //        pSizer->Add(*this);
    //    pSizer->AddObject(m_Passes);
    //    pSizer->AddObject(m_REs);
    //    pSizer->AddObject(m_RTargets);
    //}

    //SShaderTechnique(CShader* shader)
    //{
    //    m_shader = shader;
    //    uint32 i;
    //    for (i = 0; i < TTYPE_MAX; i++)
    //    {
    //        m_nTechnique[i] = -1;
    //    }
    //    for (i = 0; i < m_REs.Num(); i++)
    //    {
    //        SAFE_DELETE(m_REs[i]);
    //    }
    //    m_REs.Free();

    //    m_Flags = 0;
    //    m_nPreprocessFlags = 0;
    //    m_fProfileTime = 0;
    //}
    //SShaderTechnique& operator=(const SShaderTechnique& sl)
    //{
    //    memcpy(this, &sl, sizeof(SShaderTechnique));
    //    if (sl.m_Passes.Num())
    //    {
    //        m_Passes.Copy(sl.m_Passes);
    //        for (uint32 i = 0; i < sl.m_Passes.Num(); i++)
    //        {
    //            SShaderPass* d = &m_Passes[i];
    //            d->AddRefsToShaders();
    //        }
    //    }
    //    if (sl.m_REs.Num())
    //    {
    //        m_REs.Create(sl.m_REs.Num());
    //        for (uint32 i = 0; i < sl.m_REs.Num(); i++)
    //        {
    //            if (sl.m_REs[i])
    //                m_REs[i] = sl.m_REs[i]->mfCopyConstruct();
    //        }
    //    }

    //    return *this;
    //}

    //~SShaderTechnique()
    //{
    //    for (uint32 i = 0; i < m_Passes.Num(); i++)
    //    {
    //        SShaderPass* sl = &m_Passes[i];

    //        sl->mfFree();
    //    }
    //    for (uint32 i = 0; i < m_REs.Num(); i++)
    //    {
    //        CRenderElement* pRE = m_REs[i];
    //        pRE->Release(false);
    //    }
    //    m_REs.Free();
    //    m_Passes.Free();
    //}
    //void UpdatePreprocessFlags(CShader* pSH);

    //void* operator new(size_t Size) { void* ptr = malloc(Size); memset(ptr, 0, Size); return ptr; }
    //void* operator new(size_t Size, const std::nothrow_t& nothrow) { void* ptr = malloc(Size); if (ptr) memset(ptr, 0, Size); return ptr; }
    //void operator delete(void* Ptr) { free(Ptr); }
    //};

    ////===============================================================================

    //enum EShaderDrawType
    //{
    //    eSHDT_General,
    //    eSHDT_Light,
    //    eSHDT_Shadow,
    //    eSHDT_Terrain,
    //    eSHDT_Overlay,
    //    eSHDT_OceanShore,
    //    eSHDT_Fur,
    //    eSHDT_NoDraw,
    //    eSHDT_CustomDraw,
    //    eSHDT_Sky,
    //    eSHDT_DebugHelper,
    //};

    // General Shader structure
    public class CShader : CBaseResource, IShader
    {
        //	static CCryNameTSCRC s_sClassName;
        //public:
        //	string m_NameFile; // } FIXME: This fields order is very important
        //string m_NameShader;
        //uint32 m_NameShaderICRC;
        //EShaderDrawType m_eSHDType; // } Check CShader::operator = in ShaderCore.cpp for more info

        //uint32 m_Flags;  // Different flags EF_  (see IShader.h)
        //uint32 m_Flags2; // Different flags EF2_ (see IShader.h)
        //EVertexModifier m_nMDV;   // Vertex modificator flags

        //InputLayoutHandle m_eVertexFormat; // Base vertex format for the shader (see VertexFormats.h)
        //ECull m_eCull;         // Global culling type

        //TArray<SShaderTechnique*> m_HWTechniques;    // Hardware techniques
        //int m_nMaskCB;

        //EShaderType m_eShaderType;

        //uint64 m_nMaskGenFX;
        //SShaderGen* m_ShaderGenParams;           // BitMask params used in automatic script generation
        //SShaderTexSlots* m_ShaderTexSlots[TTYPE_MAX]; // filled out with data of the used texture slots for a given technique
        //                                              // (might be NULL if this data isn't gathered)
        //std::vector<CShader*>* m_DerivedShaders;
        //CShader* m_pGenShader;

        //int m_nRefreshFrame; // Current frame for shader reloading (to avoid multiple reloading)
        //uint32 m_SourceCRC32;
        //uint32 m_CRC32;

        ////! Minimal known distance to the object using this shader
        //float m_fMinVisibleDistance;

        ////============================================================================

        //inline int mfGetID() { return CBaseResource::GetID(); }

        //void mfFree();
        //CShader()
        //    : m_NameShaderICRC(0)
        //		, m_eSHDType(eSHDT_General)
        //		, m_Flags(0)
        //		, m_Flags2(0)
        //		, m_nMDV(MDV_NONE)
        //		, m_eVertexFormat(EDefaultInputLayouts::P3F_C4B_T2F)
        //		, m_eCull((ECull) - 1)
        //		, m_nMaskCB(0)
        //		, m_eShaderType(eST_General)
        //		, m_nMaskGenFX(0)
        //		, m_ShaderGenParams(nullptr)
        //		, m_DerivedShaders(nullptr)
        //		, m_pGenShader(nullptr)
        //		, m_nRefreshFrame(0)
        //		, m_SourceCRC32(0)
        //		, m_CRC32(0)
        //		, m_fMinVisibleDistance(FLT_MAX)
        //{
        //    memset(m_ShaderTexSlots, 0, sizeof(m_ShaderTexSlots));
        //}
        //virtual ~CShader();

        ////===================================================================================

        //// IShader interface
        //virtual int AddRef() { return CBaseResource::AddRef(); }
        //virtual int Release()
        //{
        //    if (m_Flags & EF_SYSTEM)
        //        return -1;
        //    return CBaseResource::Release();
        //}
        //virtual int ReleaseForce()
        //{
        //    m_Flags &= ~EF_SYSTEM;
        //    int nRef = 0;
        //    while (true)
        //    {
        //        nRef = Release();
        //#if !defined(_RELEASE) && defined(_DEBUG)
        //        IF(nRef < 0, 0)

        //                __debugbreak();
        //#endif
        //        if (nRef == 0)
        //            break;
        //    }
        //    return nRef;
        //}

        //virtual int GetID() { return CBaseResource::GetID(); }
        //virtual int GetRefCounter() const { return CBaseResource::GetRefCounter(); }
        //	virtual const char* GetName()             { return m_NameShader.c_str(); }
        //	virtual const char* GetName() const       { return m_NameShader.c_str(); }

        //	virtual int GetFlags() const       { return m_Flags; }
        //	virtual int GetFlags2() const      { return m_Flags2; }
        //	virtual void SetFlags2(int Flags) { m_Flags2 |= Flags; }
        //virtual void ClearFlags2(int Flags) { m_Flags2 &= ~Flags; }

        //virtual bool Reload(int nFlags, const char* szShaderName);
        //#if CRY_PLATFORM_DESKTOP
        //	virtual void mfFlushCache();
        //#endif

        //void mfFlushPendedShaders();

        //virtual int GetTechniqueID(int nTechnique, int nRegisteredTechnique)
        //{
        //    if (nTechnique < 0)
        //        nTechnique = 0;
        //    if ((int)m_HWTechniques.Num() <= nTechnique)
        //        return -1;
        //    SShaderTechnique* pTech = m_HWTechniques[nTechnique];
        //    return pTech->m_nTechnique[nRegisteredTechnique];
        //}
        //virtual TArray<CRenderElement*>* GetREs(int nTech)
        //{
        //    if (nTech < 0)
        //        nTech = 0;
        //    if (nTech < (int)m_HWTechniques.Num())
        //    {
        //        SShaderTechnique* pTech = m_HWTechniques[nTech];
        //        return &pTech->m_REs;
        //    }
        //    return NULL;
        //}
        //virtual int GetTexId();
        //virtual unsigned int GetUsedTextureTypes(void);
        //virtual InputLayoutHandle GetVertexFormat(void) { return m_eVertexFormat; }
        //virtual uint64 GetGenerationMask() { return m_nMaskGenFX; }
        //virtual ECull GetCull(void)
        //{
        //    if (m_HWTechniques.Num())
        //    {
        //        SShaderTechnique* pTech = m_HWTechniques[0];
        //        if (pTech->m_Passes.Num())
        //            return (ECull)pTech->m_Passes[0].m_eCull;
        //    }
        //    return eCULL_None;
        //}
        //virtual SShaderGen* GetGenerationParams()
        //{
        //    if (m_ShaderGenParams)
        //        return m_ShaderGenParams;
        //    if (m_pGenShader)
        //        return m_pGenShader->m_ShaderGenParams;
        //    return NULL;
        //}
        //virtual SShaderTexSlots* GetUsedTextureSlots(int nTechnique);

        //virtual DynArrayRef<SShaderParam>& GetPublicParams();
        //virtual void CopyPublicParamsTo(SInputShaderResources& copyToResource);
        //virtual EShaderType GetShaderType() { return m_eShaderType; }
        //virtual EVertexModifier GetVertexModificator() { return m_nMDV; }

        //SShaderTechnique* mfFindTechnique(const CCryNameTSCRC& name)
        //{
        //    uint32 i;
        //    for (i = 0; i < m_HWTechniques.Num(); i++)
        //    {
        //        SShaderTechnique* pTech = m_HWTechniques[i];
        //        if (pTech->m_NameCRC == name)
        //            return pTech;
        //    }
        //    return NULL;
        //}

        //SShaderTechnique* GetTechnique(int nStartTechnique, int nRequestedTechnique, bool bSilent = false);

        //virtual ITexture* GetBaseTexture(int* nPass, int* nTU);

        //CShader&          operator=(const CShader& src);
        //CTexture* mfFindBaseTexture(TArray<SShaderPass>& Passes, int* nPass, int* nTU);

        //int mfSize();

        //// All loaded shaders resources list
        //static TArray<CShaderResources*> s_ShaderResources_known;

        //virtual int Size(int Flags)
        //{
        //    return mfSize();
        //}

        //virtual void GetMemoryUsage(ICrySizer* Sizer) const;
        //void* operator new(size_t Size) { void* ptr = malloc(Size); memset(ptr, 0, Size); return ptr; }
        //void* operator new(size_t Size, const std::nothrow_t& nothrow) { void* ptr = malloc(Size); if (ptr) memset(ptr, 0, Size); return ptr; }
        //void operator delete(void* Ptr) { free(Ptr); }

        //static CCryNameTSCRC mfGetClassName()
        //{
        //    return s_sClassName;
        //}

        //void UpdateMinVisibleDistance(float fMinDistance)
        //{
        //    if (fMinDistance < m_fMinVisibleDistance)
        //    {
        //        m_fMinVisibleDistance = fMinDistance;
        //    }
        //}
        //};

        //inline SShaderTechnique* SShaderItem::GetTechnique() const
        //{
        //	int nTech = m_nTechnique;
        //	if (nTech< 0)

        //        nTech = 0;
        //	CShader* pSH = (CShader*)m_pShader;

        //	if (pSH && !pSH->m_HWTechniques.empty())
        //	{
        //		CryPrefetch(&pSH->m_HWTechniques[0]);

        //assert(m_nTechnique< 0 || pSH->m_HWTechniques.Num() == 0 || nTech<(int)pSH->m_HWTechniques.Num());
        //		if (nTech<(int)pSH->m_HWTechniques.Num())
        //			return pSH->m_HWTechniques[nTech];
        //	}
        //	return NULL;
        //}
    }
}
