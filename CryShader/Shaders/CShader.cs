using System;
using System.Collections.Generic;
using System.IO;
using FXMacro = System.Collections.Generic.Dictionary<string, CryShader.Shaders.SMacroFX>;

namespace CryShader.Shaders
{
    public struct SMacroFX
    {
        public string m_szMacro;
        public uint m_nMask;
    }

    public enum EShaderFlagType
    {
        eSFT_Global = 0,
        eSFT_Runtime,
        eSFT_MDV,
        eSFT_LT,
    }

    public enum EShaderFilterOperation
    {
        eSFO_Expand = 0,  // expand all permutations of the mask
        eSFO_And,         // and against the mask
        eSFO_Eq,          // set the mask
    }

    // includes or excludes
    public class CShaderListFilter
    {
        public bool Include = true;
        public string ShaderName;
        public class Predicate
        {
            public bool Negated = false;
            public EShaderFlagType Flags = EShaderFlagType.eSFT_Global;
            public EShaderFilterOperation Op = EShaderFilterOperation.eSFO_And;
            public ulong Mask = 0;
        }
        public List<Predicate> Predicates = new List<Predicate>();
    }

    //==================================================================================
    public partial class G
    {
        const int PD_INDEXED = 1;
        const int PD_MERGED = 4;
    }

    public class SParamDB
    {
        public string szName;
        public string szAliasNamee;
        public ECGParam eParamType = ECGParam.ECGP_Unknown;
        public uint nFlags;
        public Action<string, string, SCGParam, int, CShader> ParserFunc = null; // (string szScr, string szAnnotations, SCGParam* vpp, int nComp, CShader* ef);

        public SParamDB() { }
        public SParamDB(string inName, ECGParam ePrmType, uint inFlags)
        {
            szName = inName;
            eParamType = ePrmType;
            nFlags = inFlags;
        }
        public SParamDB(string inName, ECGParam ePrmType, uint inFlags, Action<string, string, SCGParam, int, CShader> inParserFunc)
        {
            szName = inName;
            nFlags = inFlags;
            ParserFunc = inParserFunc;
            eParamType = ePrmType;
        }
    }

    public class SSamplerDB
    {
        public string szName;
        public ECGSampler eSamplerType = ECGSampler.ECGS_Unknown;
        public uint nFlags;
        public Action<string, string, List<SFXSampler>, SCGSampler, CShader> ParserFunc; // (string szScr, string szAnnotations, std::vector<SFXSampler>* pSamplers, SCGSampler* vpp, CShader* ef);
        public SSamplerDB() { }
        public SSamplerDB(string inName, ECGSampler ePrmType, uint inFlags)
        {
            szName = inName;
            nFlags = inFlags;
            eSamplerType = ePrmType;
        }
        public SSamplerDB(string inName, ECGSampler ePrmType, uint inFlags, Action<string, string, List<SFXSampler>, SCGSampler, CShader> inParserFunc)
        {
            szName = inName;
            nFlags = inFlags;
            ParserFunc = inParserFunc;
            eSamplerType = ePrmType;
        }
    }

    public class STextureDB
    {
        public string szName;
        public ECGTexture eTextureType = ECGTexture.ECGT_Unknown;
        public uint nFlags;
        Action<string, string, List<SFXTexture>, SCGTexture, CShader> ParserFunc; //(string szScr, string szAnnotations, std::vector<SFXTexture>* pSamplers, SCGTexture* vpp, CShader* ef);
        public STextureDB() { }
        public STextureDB(string inName, ECGTexture ePrmType, uint inFlags)
        {
            szName = inName;
            nFlags = inFlags;
            eTextureType = ePrmType;
        }
        public STextureDB(string inName, ECGTexture ePrmType, uint inFlags, Action<string, string, List<SFXTexture>, SCGTexture, CShader> inParserFunc)
        {
            szName = inName;
            nFlags = inFlags;
            ParserFunc = inParserFunc;
            eTextureType = ePrmType;
        }
    }

    public enum EShaderCacheMode
    {
        eSC_Normal = 0,
        eSC_BuildGlobal = 2,
        eSC_BuildGlobalList = 3,
        eSC_Preactivate = 4,
    }


    //////////////////////////////////////////////////////////////////////////
    public class CShaderMan // : ISystemEventListener // CShaderSerialize
    {



        //////////////////////////////////////////////////////////////////////////
        // ISystemEventListener interface implementation.
        //////////////////////////////////////////////////////////////////////////
        //virtual void OnSystemEvent(ESystemEvent event, UINT_PTR wparam, UINT_PTR lparam);
        //////////////////////////////////////////////////////////////////////////

        private void mfUpdateBuildVersion(string szCachePath) => throw new NotImplementedException();

        private STexAnim mfReadTexSequence(string name, int Flags, bool bFindOnly) => throw new NotImplementedException();
        private int mfReadTexSequence(STexSamplerRT smp, string name, int Flags, bool bFindOnly) => throw new NotImplementedException();

        private CShader mfNewShader(string szName) => throw new NotImplementedException();

        private bool mfCompileShaderGen(SShaderGen shg, string scr) => throw new NotImplementedException();
        private SShaderGenBit mfCompileShaderGenProperty(string scr) => throw new NotImplementedException();

        private void mfSetResourceTexState(SEfResTexture Tex) => throw new NotImplementedException();
        private CTexture mfTryToLoadTexture(string nameTex, STexSamplerRT smp, int Flags, bool bFindOnly) => throw new NotImplementedException();
        private void mfRefreshResourceTextureConstants(EEfResTextures Id, out SInputShaderResources RS) => throw new NotImplementedException();
        private void mfRefreshResourceTextureConstants(EEfResTextures Id, out CShaderResources RS) => throw new NotImplementedException();
        private CTexture mfFindResourceTexture(string nameTex, string path, int Flags, SEfResTexture Tex) => throw new NotImplementedException();
        private CTexture mfLoadResourceTexture(string nameTex, string path, int Flags, SEfResTexture Tex) => throw new NotImplementedException();
        private bool mfLoadResourceTexture(EEfResTextures Id, ref SInputShaderResources RS, uint CustomFlags, bool bReplaceMeOnFail = false) => throw new NotImplementedException();
        private bool mfLoadResourceTexture(EEfResTextures Id, ref CShaderResources RS, uint CustomFlags, bool bReplaceMeOnFail = false) => throw new NotImplementedException();
        private void mfLoadDefaultTexture(EEfResTextures Id, ref CShaderResources RS, EEfResTextures Alias) => throw new NotImplementedException();
        private void mfCheckShaderResTextures(ref SShaderPass[] Dst, CShader ef, CShaderResources Res) => throw new NotImplementedException();
        private void mfCheckShaderResTexturesHW(ref SShaderPass[] Dst, CShader ef, CShaderResources Res) => throw new NotImplementedException();
        private CTexture mfCheckTemplateTexName(string mapname, ETEX_Type eTT) => throw new NotImplementedException();

        private CShader mfCompile(CShader ef, string scr) => throw new NotImplementedException();

        private void mfRefreshResources(CShaderResources Res, SLoadShaderItemArgs pArgs = null) => throw new NotImplementedException();

        private bool mfReloadShaderFile(string szName, int nFlags) => throw new NotImplementedException();


        private static void FilterShaderCacheGenListForOrbis(FXShaderCacheCombinations combinations) => throw new NotImplementedException();

        public string m_pCurScript;
        public CShaderManBin m_Bin;
        public CResFileLookupDataMan[] m_ResLookupDataMan = new CResFileLookupDataMan[2];  // cacheSource::readonly, cacheSource::user

        public string mfTemplateTexIdToName(int Id) => throw new NotImplementedException();
        public SShaderGenComb mfGetShaderGenInfo(string nmFX) => throw new NotImplementedException();

        public bool mfReloadShaderIncludes(string szPath, int nFlags) => throw new NotImplementedException();
        public bool mfReloadAllShaders(int nFlags, uint nFlagsHW, int currentFrameID) => throw new NotImplementedException();
        public bool mfReloadFile(string szPath, string szName, int nFlags) => throw new NotImplementedException();

        public void ParseShaderProfiles() => throw new NotImplementedException();
        public void ParseShaderProfile(string scr, SShaderProfile pr) => throw new NotImplementedException();

        public EEfResTextures mfCheckTextureSlotName(string mapname) => throw new NotImplementedException();
        public SParamDB mfGetShaderParamDB(string szSemantic) => throw new NotImplementedException();
        public string mfGetShaderParamName(ECGParam ePR) => throw new NotImplementedException();
        public bool mfParseParamComp(int comp, SCGParam pCurParam, string szSemantic, string params_, string szAnnotations, SShaderFXParams FXParams, CShader ef, uint nParamFlags, EHWShaderClass eSHClass, bool bExpressionOperand) => throw new NotImplementedException();
        public bool mfParseCGParam(string scr, string szAnnotations, SShaderFXParams FXParams, CShader ef, List<SCGParam> pParams, int nComps, uint nParamFlags, EHWShaderClass eSHClass, bool bExpressionOperand) => throw new NotImplementedException();
        public bool mfParseFXParameter(SShaderFXParams FXParams, SFXParam pr, string ParamName, CShader ef, bool bInstParam, int nParams, List<SCGParam> pParams, EHWShaderClass eSHClass, bool bExpressionOperand) => throw new NotImplementedException();

        public bool mfParseFXTexture(SShaderFXParams FXParams, SFXTexture pr, string ParamName, CShader ef, int nParams, List<SCGTexture> pParams, EHWShaderClass eSHClass) => throw new NotImplementedException();
        public bool mfParseFXSampler(SShaderFXParams FXParams, SFXSampler pr, string ParamName, CShader ef, int nParams, List<SCGSampler> pParams, EHWShaderClass eSHClass) => throw new NotImplementedException();

        public void mfCheckObjectDependParams(List<SCGParam> PNoObj, List<SCGParam> PObj, EHWShaderClass eSH, CShader pFXShader) => throw new NotImplementedException();

        public void mfBeginFrame() => throw new NotImplementedException();

        public void mfGetShaderListPath(string nameOut, int nType) => throw new NotImplementedException();


        //
        public bool m_bInitialized;
        public bool m_bLoadedSystem;

        public string m_ShadersGamePath;
        public string m_ShadersGameExtPath;
        public string m_ShadersPath;
        public string m_ShadersExtPath;
        public string m_ShadersCache;
        public string m_ShadersFilter;
        public string m_ShadersMergeCachePath;
        public string m_szUserPath;

        public int m_nFrameForceReload;

        public string m_HWPath;

        public CShader m_pCurShader;
        public static SResourceContainer s_pContainer;  // List/Map of objects for shaders resource class

        public List<string> m_ShaderNames = new List<string>();

        public static string s_cNameHEAD;

        public static CShader s_DefaultShader;
        public static CShader s_shPostEffects;      // engine specific post process effects
        public static CShader s_shPostDepthOfField; // depth of field
        public static CShader s_shPostMotionBlur;
        public static CShader s_shPostSunShafts;
        public static CShader s_sh3DHUD;

        // Deferred rendering passes
        public static CShader s_shDeferredShading;
        public static CShader s_ShaderDeferredCaustics;
        public static CShader s_ShaderDeferredRain;
        public static CShader s_ShaderDeferredSnow;

        public static CShader s_ShaderFPEmu;
        public static CShader s_ShaderFallback;
        public static CShader s_ShaderScaleForm;
        public static CShader s_ShaderStars;
        public static CShader s_ShaderShadowBlur;
        public static CShader s_ShaderShadowMaskGen;
#if FEATURE_SVO_GI
        public static CShader s_ShaderSVOGI;
#endif
        public static CShader s_shHDRPostProcess;
        public static CShader s_shPostEffectsGame; // game specific post process effects
        public static CShader s_shPostEffectsRenderModes;
        public static CShader s_shPostAA;
        public static CShader s_ShaderDebug;
        public static CShader s_ShaderLensOptics;
        public static CShader s_ShaderSoftOcclusionQuery;
        public static CShader s_ShaderLightStyles;
        public static CShader s_ShaderCommon;
        public static CShader s_ShaderOcclTest;
        public static CShader s_ShaderDXTCompress;
        public static CShader s_ShaderStereo;
        public static CShader s_ShaderClouds;
        public static CShader s_ShaderMobileComposition;
        public static CShader s_ShaderGpuParticles;

        public SInputShaderResources m_pCurInputResources;
        public SShaderGen m_pGlobalExt;

        public List<SShaderGenComb> m_SGC = new List<SShaderGenComb>();

        public int m_nCombinationsProcess;
        public int m_nCombinationsProcessOverall;
        public int m_nCombinationsCompiled;
        public int m_nCombinationsEmpty;

        public EShaderCacheMode m_eCacheMode;

        public string m_szShaderPrecache;

        public FXShaderCacheCombinations[] m_ShaderCacheCombinations = new FXShaderCacheCombinations[2];
        public FXShaderCacheCombinations m_ShaderCacheExportCombinations;
        public FileStream[] m_FPCacheCombinations = new FileStream[2];

        //public typedef std::vector<CCryNameTSCRC, stl::STLGlobalAllocator<CCryNameTSCRC>> ShaderCacheMissesVec;
        public ShaderCacheMissesVec m_ShaderCacheMisses;
        public string m_ShaderCacheMissPath;
        public ShaderCacheMissCallback m_ShaderCacheMissCallback;

        public SShaderCacheStatistics m_ShaderCacheStats;

        public uint m_nFrameLastSubmitted;
        public uint m_nFrameSubmit;
        public SShaderProfile[] m_ShaderProfiles = new SShaderProfile[eST_Max];
        public SShaderProfile[] m_ShaderFixedProfiles = new SShaderProfile[eSQ_Max];

        public int m_bActivated;

        public CShaderParserHelper m_shaderParserHelper;

        public bool m_bReload;

        // Shared common global flags data

        // Map used for storing automatically-generated flags and mapping old shader names masks to generated ones
        //  map< shader flag names, mask >
        //public typedef std::map<string, ulong> MapNameFlags;
        //public typedef MapNameFlags::iterator   MapNameFlagsItor;
        public MapNameFlags m_pShaderCommonGlobalFlag;

        public MapNameFlags m_pSCGFlagLegacyFix;
        public ulong m_nSGFlagsFix;

        // Map stored for convenience mapping betweens old flags and new ones
        //  map < shader name , map< shader flag names, mask > >
        //public typedef std::map<string, MapNameFlags*> ShaderMapNameFlags;
        //public typedef ShaderMapNameFlags::iterator    ShaderMapNameFlagsItor;
        public ShaderMapNameFlags m_pShadersGlobalFlags;

        //public typedef std::map<CCryNameTSCRC, SShaderGen*> ShaderExt;
        //public typedef ShaderExt::iterator                  ShaderExtItor;
        public ShaderExt m_ShaderExts;

        // Concatenated list of shader names using automatic masks generation
        public string m_pShadersRemapList;

        // Helper functors for cleaning up

        //      struct SShaderMapNameFlagsContainerDelete
        //      {
        //          void operator()(ShaderMapNameFlags::value_type& pObj)
        //{
        //	SAFE_DELETE(pObj.second);
        //      }
        //  }

        public CShaderMan() => throw new NotImplementedException();

        public void ShutDown() => throw new NotImplementedException();
        public void mfReleaseShaders() => throw new NotImplementedException();

        public SShaderGen mfCreateShaderGenInfo(string szName, bool bRuntime) => throw new NotImplementedException();
        public void mfRemapShaderGenInfoBits(string szName, SShaderGen pShGen) => throw new NotImplementedException();

        public ulong mfGetRemapedShaderMaskGen(string szName, ulong nMaskGen = 0, out bool bFixup) => throw new NotImplementedException();
        public string mfGetShaderBitNamesFromMaskGen(string szName, ulong nMaskGen) => throw new NotImplementedException();

        public bool mfUsesGlobalFlags(string szShaderName) => throw new NotImplementedException();
        public string mfGetShaderBitNamesFromGlobalMaskGen(ulong nMaskGen) => throw new NotImplementedException();
        public ulong mfGetShaderGlobalMaskGenFromString(string szShaderGen) => throw new NotImplementedException();

        public void mfInitGlobal() => throw new NotImplementedException();
        public void mfInitLookups() => throw new NotImplementedException();

        public void mfPreloadShaderExts() => throw new NotImplementedException();
        public void mfInitCommonGlobalFlags() => throw new NotImplementedException();
        public void mfInitCommonGlobalFlagsLegacyFix() => throw new NotImplementedException();
        public bool mfRemapCommonGlobalFlagsWithLegacy() => throw new NotImplementedException();
        public void mfCreateCommonGlobalFlags(szName) => throw new NotImplementedException();
        public void mfSaveCommonGlobalFlagsToDisk(szName, uint nMaskCount) => throw new NotImplementedException();

        public void mfInit() => throw new NotImplementedException();
        public void mfPostInit() => throw new NotImplementedException();
        public void mfSortResources() => throw new NotImplementedException();
        public CShaderResources mfCreateShaderResources(SInputShaderResources Res, bool bShare) => throw new NotImplementedException();
        public bool mfRefreshResourceConstants(CShaderResources Res) => throw new NotImplementedException();
        public bool mfRefreshResourceConstants(SShaderItem SI) { return mfRefreshResourceConstants((CShaderResources)SI.m_pShaderResources); }
        public bool mfUpdateTechnik(SShaderItem SI, string Name) => throw new NotImplementedException();
        public SShaderItem mfShaderItemForName(string szName, bool bShare, int flags, SInputShaderResources Res = null, ulong nMaskGen = 0, IRenderer.SLoadShaderItemArgs pArgs = 0) => throw new NotImplementedException();
        public CShader mfForName(string name, int flags, CShaderResources Res = null, ulong nMaskGen = 0) => throw new NotImplementedException();

        public bool mfRefreshSystemShader(string szName, CShader pSysShader)
        {
            if (pSysShader == null)
            {
                CryComment("Load System Shader '%s'...", szName);

                if ((pSysShader = mfForName(szName, EF_SYSTEM)))
                    return true;
            }

            return false;
        }

        public void RT_ParseShader(CShader pSH, ulong nMaskGen, uint flags, CShaderResources Res) => throw new NotImplementedException();
        public void RT_SetShaderQuality(EShaderType eST, EShaderQuality eSQ) => throw new NotImplementedException();

        public void CreateShaderMaskGenString(CShader pSH, stack_string flagString) => throw new NotImplementedException();
        public void CreateShaderExportRequestLine(CShader pSH, stack_string exportString) => throw new NotImplementedException();

        public SFXParam mfGetFXParameter(List<SFXParam> Params, string param) => throw new NotImplementedException();
        public SFXSampler mfGetFXSampler(List<SFXSampler> Params, string param) => throw new NotImplementedException();
        public SFXTexture mfGetFXTexture(List<SFXTexture> Params, string param) => throw new NotImplementedException();
        public string mfParseFX_Parameter(string buf, EParamType eType, string szName) => throw new NotImplementedException();
        public void mfParseFX_Annotations_Script(string buf, CShader ef, List<SFXStruct> Structs, ref bool bPublic, string[] techStart) => throw new NotImplementedException();
        public void mfParseFX_Annotations(string buf, CShader ef, List<SFXStruct> Structs, ref bool bPublic, string[] techStart) => throw new NotImplementedException();
        public void mfParseFXTechnique_Annotations_Script(string buf, CShader ef, List<SFXStruct> Structs, SShaderTechnique pShTech, ref bool bPublic, List<SShaderTechParseParams> techParams) => throw new NotImplementedException();
        public void mfParseFXTechnique_Annotations(string buf, CShader ef, List<SFXStruct> Structs, SShaderTechnique pShTech, ref bool bPublic, List<SShaderTechParseParams> techParams) => throw new NotImplementedException();
        public void mfParseFX_Global(SFXParam pr, CShader ef, List<SFXStruct> Structs, string[] techStart) => throw new NotImplementedException();
        public bool mfParseDummyFX_Global(List<SFXStruct> Structs, string annot, string[] techStart) => throw new NotImplementedException();
        public string mfParseFXTechnique_GenerateShaderScript(List<SFXStruct> Structs, FXMacro Macros, List<SFXParam> Params, List<SFXParam> AffectedParams, string szEntryFunc, CShader ef, EHWShaderClass eSHClass, string szShaderName, uint nAffectMask, string szType) => throw new NotImplementedException();
        public bool mfParseFXTechnique_MergeParameters(List<SFXStruct> Structs, List<SFXParam> Params, List<int> AffectedFunc, SFXStruct pMainFunc, CShader ef, EHWShaderClass eSHClass, string szShaderName, List<SFXParam> NewParams) => throw new NotImplementedException();
        public CTexture mfParseFXTechnique_LoadShaderTexture(STexSamplerRT smp, string szName, SShaderPass pShPass, CShader ef, int nIndex, byte ColorOp, byte AlphaOp, byte ColorArg, byte AlphaArg) => throw new NotImplementedException();
        public bool mfParseFXTechnique_CustomRE(string buf, string name, SShaderTechnique pShTech, CShader ef) => throw new NotImplementedException();
        public bool mfParseLightStyle(CLightStyle ls, string buf) => throw new NotImplementedException();
        public bool mfParseFXLightStyle(string buf, int nID, CShader ef, List<SFXStruct> Structs) => throw new NotImplementedException();
        public CShader mfParseFX(string buf, CShader ef, CShader efGen, ulong nMaskGen) => throw new NotImplementedException();
        public void mfPostLoadFX(CShader efT, List<SShaderTechParseParams> techParams, string[] techStart) => throw new NotImplementedException();
        public bool mfParseDummyFX(string buf, List<string> ShaderNames, string szName) => throw new NotImplementedException();
        public bool mfAddFXShaderNames(string szName, List<string> ShaderNames, bool bUpdateCRC) => throw new NotImplementedException();
        public bool mfInitShadersDummy(bool bUpdateCRC) => throw new NotImplementedException();

        public ulong mfGetRTForName(string buf) => throw new NotImplementedException();
        public uint mfGetGLForName(string buf, CShader ef) => throw new NotImplementedException();

        public void mfFillGenMacroses(SShaderGen shG, byte[] buf, ulong nMaskGen) => throw new NotImplementedException();
        public bool mfModifyGenFlags(CShader efGen, CShaderResources pRes, ref ulong nMaskGen, ref ulong nMaskGenHW) => throw new NotImplementedException();

        public bool mfGatherShadersList(string szPath, bool bCheckIncludes, bool bUpdateCRC, List<string> Names) => throw new NotImplementedException();
        public void mfGatherFilesList(string szPath, List<string> Names, int nLevel, bool bUseFilter, bool bMaterial = false) => throw new NotImplementedException();
        public int mfInitShadersList(List<string> ShaderNames) => throw new NotImplementedException();
        public void mfSetDefaults() => throw new NotImplementedException();
        public void mfReleaseSystemShaders() => throw new NotImplementedException();
        public void mfLoadBasicSystemShaders() => throw new NotImplementedException();
        public void mfLoadDefaultSystemShaders() => throw new NotImplementedException();
        public void mfCloseShadersCache(int nID) => throw new NotImplementedException();
        public void mfInitShadersCacheMissLog() => throw new NotImplementedException();

        public void mfInitShadersCache(byte bForLevel, FXShaderCacheCombinations Combinations = null, string pCombinations = null, int nType = 0) => throw new NotImplementedException();
        public void mfMergeShadersCombinations(FXShaderCacheCombinations Combinations, int nType) => throw new NotImplementedException();
        public void mfInsertNewCombination(SShaderCombIdent Ident, EHWShaderClass eCL, string name, int nID, string Str = null, byte bStore = 1) => throw new NotImplementedException();
        public string mfGetShaderCompileFlags(EHWShaderClass eClass, UPipelineState pipelineState) => throw new NotImplementedException();

        public bool mfPreloadBinaryShaders() => throw new NotImplementedException();

        public bool LoadShaderStartupCache() => throw new NotImplementedException();
        public void UnloadShaderStartupCache() => throw new NotImplementedException();

#if CRY_PLATFORM_DESKTOP
            void AddCombination(SCacheCombination& cmb, FXShaderCacheCombinations& CmbsMap, CHWShader* pHWS);
            void AddGLCombinations(CShader* pSH, std::vector<SCacheCombination>& CmbsGL);
            void AddLTCombinations(SCacheCombination& cmb, FXShaderCacheCombinations& CmbsMap, CHWShader* pHWS);
            void AddRTCombinations(FXShaderCacheCombinations& CmbsMap, CHWShader* pHWS, CShader* pSH, FXShaderCacheCombinations* Combinations);
            void AddGLCombination(FXShaderCacheCombinations& CmbsMap, SCacheCombination& cc);
            void FilterShaderCombinations(std::vector<SCacheCombination>& Cmbs, const std::vector<CShaderListFilter>& Filters);
            void mfPrecacheShaders(bool bStatsOnly);
            void _PrecacheShaderList(bool bStatsOnly);

            void mfAddRTCombinations(FXShaderCacheCombinations& CmbsMapSrc, FXShaderCacheCombinations& CmbsMapDst, CHWShader* pSH, bool bListOnly);
            void mfAddRTCombination_r(int nComb, FXShaderCacheCombinations& CmbsMapDst, SCacheCombination* cmb, CHWShader* pSH, bool bAutoPrecache);
            void mfAddLTCombinations(SCacheCombination* cmb, FXShaderCacheCombinations& CmbsMapDst);
            void mfAddLTCombination(SCacheCombination* cmb, FXShaderCacheCombinations& CmbsMapDst, DWORD dwL);
#endif


        public int Size() = -1;
        //{
        //    int nSize = 0;// sizeof(this);
        //    nSize += m_SGC.capacity();
        //    nSize += m_Bin.Size();
        //    return nSize;
        //}

        public void GetMemoryUsage(ICrySizer pSizer)
        {
            pSizer.AddObject(m_Bin);
            pSizer.AddObject(m_SGC);
            pSizer.AddObject(m_ShaderNames);
            pSizer.AddObject(m_ShaderCacheCombinations[0]);
            pSizer.AddObject(m_ShaderCacheCombinations[1]);
        }

        public static float EvalWaveForm(SWaveForm wf) => throw new NotImplementedException();
        public static float EvalWaveForm(SWaveForm2 wf) => throw new NotImplementedException();
        public static float EvalWaveForm2(SWaveForm wf, float frac) => throw new NotImplementedException();
    }
}
