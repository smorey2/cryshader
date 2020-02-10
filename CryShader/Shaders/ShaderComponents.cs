using System;
using Vec3 = UnityEngine.Vector3;
using Vec4 = UnityEngine.Vector4;

namespace CryShader.Shaders
{
    partial class G
    {
        const uint PF_LOCAL = 1;
        const uint PF_SINGLE_COMP = 2;
        const uint PF_DONTALLOW_DYNMERGE = 4;
        const uint PF_INTEGER = 8;
        const uint PF_BOOL = 0x10;
        const uint PF_POSITION = 0x20;
        const uint PF_MATRIX = 0x40;
        const uint PF_SCALAR = 0x80;
        const uint PF_TWEAKABLE_0 = 0x100;
        const uint PF_TWEAKABLE_1 = 0x200;
        const uint PF_TWEAKABLE_2 = 0x400;
        const uint PF_TWEAKABLE_3 = 0x800;
        const uint PF_TWEAKABLE_MASK = 0xf00;
        const uint PF_MERGE_MASK = 0xff000;
        const uint PF_MERGE_SHIFT = 12;
        const uint PF_INSTANCE = 0x100000;
        const uint PF_MATERIAL = 0x200000;
        // unused = 0x400000;
        // unused = 0x800000;
        const uint PF_CUSTOM_BINDED = 0x1000000;
        const uint PF_CANMERGED = 0x2000000;
        const uint PF_AUTOMERGED = 0x4000000;
        const uint PF_ALWAYS = 0x8000000;
        const uint PF_GLOBAL = 0x10000000;
    }

    public enum ECGParam
    {
        ECGP_Unknown,

        ECGP_Matr_PI_ObjOrigComposite,
        ECGP_PB_VisionMtlParams,
        ECGP_PI_EffectLayerParams,
        ECGP_PI_NumInstructions,

        ECGP_PI_WrinklesMask0,
        ECGP_PI_WrinklesMask1,
        ECGP_PI_WrinklesMask2,

        ECGP_PB_Scalar,
        ECGP_PM_Tweakable,

        ECGP_PM_MatChannelSB,
        ECGP_PM_MatDiffuseColor,
        ECGP_PM_MatSpecularColor,
        ECGP_PM_MatEmissiveColor,
        ECGP_PM_MatMatrixTCM,
        ECGP_PM_MatDeformWave,
        ECGP_PM_MatDetailTilingAndAlphaRef,
        ECGP_PM_MatSilPomDetailParams,

        ECGP_PB_HDRParams,
        ECGP_PB_StereoParams,

        ECGP_PB_IrregKernel,
        ECGP_PB_RegularKernel,

        ECGP_PB_VolumetricFogParams,
        ECGP_PB_VolumetricFogRampParams,
        ECGP_PB_VolumetricFogSunDir,
        ECGP_PB_FogColGradColBase,
        ECGP_PB_FogColGradColDelta,
        ECGP_PB_FogColGradParams,
        ECGP_PB_FogColGradRadial,
        ECGP_PB_VolumetricFogSamplingParams,
        ECGP_PB_VolumetricFogDistributionParams,
        ECGP_PB_VolumetricFogScatteringParams,
        ECGP_PB_VolumetricFogScatteringBlendParams,
        ECGP_PB_VolumetricFogScatteringColor,
        ECGP_PB_VolumetricFogScatteringSecondaryColor,
        ECGP_PB_VolumetricFogHeightDensityParams,
        ECGP_PB_VolumetricFogHeightDensityRampParams,
        ECGP_PB_VolumetricFogDistanceParams,
        ECGP_PB_VolumetricFogGlobalEnvProbe0,
        ECGP_PB_VolumetricFogGlobalEnvProbe1,

        ECGP_PB_FromRE,

        ECGP_PB_WaterLevel,
        ECGP_PB_CausticsParams,
        ECGP_PB_CausticsSmoothSunDirection,

        ECGP_PB_Time,
        ECGP_PB_FrameTime,
        ECGP_PB_CameraPos,
        ECGP_PB_ScreenSize,
        ECGP_PB_NearFarDist,

        ECGP_PB_CloudShadingColorSun,
        ECGP_PB_CloudShadingColorSky,

        ECGP_PB_CloudShadowParams,
        ECGP_PB_CloudShadowAnimParams,

        ECGP_PB_ClipVolumeParams,

        ECGP_PB_WaterRipplesLookupParams,

#if FEATURE_SVO_GI
	    ECGP_PB_SvoViewProj0,
	    ECGP_PB_SvoViewProj1,
	    ECGP_PB_SvoViewProj2,
	    ECGP_PB_SvoNodeBoxWS,
	    ECGP_PB_SvoNodeBoxTS,
	    ECGP_PB_SvoNodesForUpdate0,
	    ECGP_PB_SvoNodesForUpdate1,
	    ECGP_PB_SvoNodesForUpdate2,
	    ECGP_PB_SvoNodesForUpdate3,
	    ECGP_PB_SvoNodesForUpdate4,
	    ECGP_PB_SvoNodesForUpdate5,
	    ECGP_PB_SvoNodesForUpdate6,
	    ECGP_PB_SvoNodesForUpdate7,

	    ECGP_PB_SvoTreeSettings0,
	    ECGP_PB_SvoTreeSettings1,
	    ECGP_PB_SvoTreeSettings2,
	    ECGP_PB_SvoTreeSettings3,
	    ECGP_PB_SvoTreeSettings4,
	    ECGP_PB_SvoTreeSettings5,
	    ECGP_PB_SvoParams0,
	    ECGP_PB_SvoParams1,
	    ECGP_PB_SvoParams2,
	    ECGP_PB_SvoParams3,
	    ECGP_PB_SvoParams4,
	    ECGP_PB_SvoParams5,
	    ECGP_PB_SvoParams6,
	    ECGP_PB_SvoParams7,
	    ECGP_PB_SvoParams8,
	    ECGP_PB_SvoParams9,
#endif

        ECGP_COUNT,
    }

    // Constants for RenderView to be set in shaders
    public class SRenderViewShaderConstants
    {
        public int nFrameID;

        public Vec3 vWaterLevel;           // ECGP_PB_WaterLevel *
        public float fHDRDynamicMultiplier; // ECGP_PB_HDRDynamicMultiplier *

        public Vec4 pFogColGradColBase;  // ECGP_PB_FogColGradColBase *
        public Vec4 pFogColGradColDelta; // ECGP_PB_FogColGradColDelta *
        public Vec4 pFogColGradParams;   // ECGP_PB_FogColGradParams *
        public Vec4 pFogColGradRadial;   // ECGP_PB_FogColGradRadial *

        public Vec4 pVolumetricFogParams;     // ECGP_PB_VolumetricFogParams *
        public Vec4 pVolumetricFogRampParams; // ECGP_PB_VolumetricFogRampParams *
        public Vec4 pVolumetricFogSunDir;     // ECGP_PB_VolumetricFogSunDir *

        public Vec3 pCameraPos;   //ECGP_PF_CameraPos

        public Vec3 pCausticsParams; //ECGP_PB_CausticsParams *
        public Vec3 pSunColor;       //ECGP_PF_SunColor *
        public float sunSpecularMultiplier;
        public Vec3 pSkyColor;       //ECGP_PF_SkyColor *
        public Vec3 pSunDirection;   //ECGP_PF_SunDirection *

        public Vec3 pCloudShadingColorSun; //ECGP_PB_CloudShadingColorSun *
        public Vec3 pCloudShadingColorSky; //ECGP_PB_CloudShadingColorSky *

        public Vec4 pCloudShadowParams;     //ECGP_PB_CloudShadowParams *
        public Vec4 pCloudShadowAnimParams; //ECGP_PB_CloudShadowAnimParams *
        public Vec4 pScreenspaceShadowsParams;

        public Vec4 post3DRendererAmbient;

        public Vec3 vCausticsCurrSunDir;
        public int nCausticsFrameID;

        public Vec3 pVolCloudTilingSize;
        public Vec3 pVolCloudTilingOffset;

        public Vec4 pVolumetricFogSamplingParams;           // ECGP_PB_VolumetricFogSamplingParams
        public Vec4 pVolumetricFogDistributionParams;       // ECGP_PB_VolumetricFogDistributionParams
        public Vec4 pVolumetricFogScatteringParams;         // ECGP_PB_VolumetricFogScatteringParams
        public Vec4 pVolumetricFogScatteringBlendParams;    // ECGP_PB_VolumetricFogScatteringParams
        public Vec4 pVolumetricFogScatteringColor;          // ECGP_PB_VolumetricFogScatteringColor
        public Vec4 pVolumetricFogScatteringSecondaryColor; // ECGP_PB_VolumetricFogScatteringSecondaryColor
        public Vec4 pVolumetricFogHeightDensityParams;      // ECGP_PB_VolumetricFogHeightDensityParams
        public Vec4 pVolumetricFogHeightDensityRampParams;  // ECGP_PB_VolumetricFogHeightDensityRampParams
        public Vec4 pVolumetricFogDistanceParams;           // ECGP_PB_VolumetricFogDistanceParams

        public float[][] irregularFilterKernel = new float[8][]; //:[4]
    }

    public enum EOperation
    {
        eOp_Unknown,
        eOp_Add,
        eOp_Sub,
        eOp_Div,
        eOp_Mul,
        eOp_Log,
    }

    public class SCGBind
    {
        public string m_Name;
        public uint m_Flags;
        public short m_dwBind;
        public short m_dwCBufSlot;
        public int m_nParameters;
        public SCGBind()
        {
            m_nParameters = 1;
            m_dwBind = -2;
            m_dwCBufSlot = 0;
            m_Flags = 0;
        }
        public SCGBind(SCGBind sb)
        {
            m_Name = sb.m_Name;
            m_Flags = sb.m_Flags;
            m_dwBind = sb.m_dwBind;
            m_dwCBufSlot = sb.m_dwCBufSlot;
            m_nParameters = sb.m_nParameters;
        }
        //SCGBind operator=(SCGBind sb)
        //{
        //	this->~SCGBind();
        //	new(this)SCGBind(sb);
        //	return *this;
        //}
        //int Size() => sizeof(SCGBind);
        //void GetMemoryUsage(ICrySizer pSizer) { }
    }

    public unsafe struct SVertexInputStream
    {
        fixed char semanticName[14];
        byte semanticIndex;
        byte attributeLocation;

        public SVertexInputStream(string streamSemanticName, byte streamSemanticIndex, byte streamAttributeLocation)
        {
            //      CRY_ASSERT(strlen(streamSemanticName) < CRY_ARRAY_COUNT(semanticName) - 1);
            //      strncpy(semanticName, streamSemanticName, CRY_ARRAY_COUNT(semanticName) - 1);
            semanticIndex = streamSemanticIndex;
            attributeLocation = streamAttributeLocation;
        }
    }

    public class SParamData
    {
        string[] m_CompNames = new string[4];
        //      union UData
        //      {
        //          uint64 nData64 [4];
        //          uint32 nData32 [4];
        //	float fData[4];
        //} d;
        //public SParamData() { }
        //   SParamData(SParamData sp);
        //   SParamData operator=(SParamData sp)
        //   {
        //       this->~SParamData();
        //       new (this)SParamData(sp);
        //       return *this;
        //   }
        //int Size() => sizeof(SParamData);
        //void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    pSizer->AddObject(this, sizeof(*this));
        //}
    }

    public struct SCGLiteral
    {
        int m_nIndex;
        //Vec4 m_vVec;
        //int Size() => sizeof(SCGLiteral);
        //void GetMemoryUsage(ICrySizer pSizer) { }
    }

    public class SCGParam : SCGBind
    {
        ECGParam m_eCGParamType = ECGParam.ECGP_Unknown;
        SParamData m_pData;
        IntPtr m_nID;
        SCGParam() { }
        SCGParam(SCGParam sp) : base(sp)
        {
            m_eCGParamType = sp.m_eCGParamType;
            m_nID = sp.m_nID;
            if (sp.m_pData != null)
            {
                m_pData = new SParamData();
                m_pData = sp.m_pData;
            }
            else m_pData = null;
        }
        //   SCGParam operator=(SCGParam sp)
        //{
        // this->~SCGParam();
        //       new(this)SCGParam(sp);
        // return *this;
        //}
        //   bool operator !=(const SCGParam& sp) const
        //{
        // if (sp.m_dwBind == m_dwBind &&
        //  sp.m_Name == m_Name &&
        //  sp.m_nID == m_nID &&
        //  sp.m_nParameters == m_nParameters &&
        //  sp.m_eCGParamType == m_eCGParamType &&
        //  sp.m_dwCBufSlot == m_dwCBufSlot &&
        //  sp.m_Flags == m_Flags &&
        //  !sp.m_pData && !m_pData)
        // {
        //  return false;
        // }
        // return true;
        //}
        //string GetParamCompName(int nComp)
        //{
        //	if (!m_pData)
        //		return "None";
        //	return m_pData->m_CompNames[nComp];
        //}
        //int Size()
        //   {
        //       int nSize = sizeof(SCGParam);
        //       if (m_pData)
        //           nSize += m_pData->Size();
        //       return nSize;
        //   }
        //   void GetMemoryUsage(ICrySizer pSizer)
        //{
        //	pSizer->AddObject(m_pData);
        //}
    }

    public enum ECGSampler
    {
        ECGS_Unknown,
        ECGS_MatSlot_Diffuse,
        ECGS_MatSlot_Normalmap,
        ECGS_MatSlot_Gloss,
        ECGS_MatSlot_Env,
        ECGS_Shadow0,
        ECGS_Shadow1,
        ECGS_Shadow2,
        ECGS_Shadow3,
        ECGS_Shadow4,
        ECGS_Shadow5,
        ECGS_Shadow6,
        ECGS_Shadow7,
        ECGS_TrilinearClamp,
        ECGS_TrilinearWrap,
        ECGS_MatAnisoHighWrap,
        ECGS_MatAnisoLowWrap,
        ECGS_MatTrilinearWrap,
        ECGS_MatBilinearWrap,
        ECGS_MatTrilinearClamp,
        ECGS_MatBilinearClamp,
        ECGS_MatAnisoHighBorder,
        ECGS_MatTrilinearBorder,
        ECGS_PointWrap,
        ECGS_PointClamp,
        ECGS_COUNT
    }

    public class SCGSampler : SCGBind
    {
        public SamplerStateHandle m_nStateHandle;
        public ECGSampler m_eCGSamplerType = ECGSampler.ECGS_Unknown;
        public SCGSampler() { }
        public SCGSampler(SCGSampler sp) : base(sp)
        {
            m_eCGSamplerType = sp.m_eCGSamplerType;
            m_nStateHandle = sp.m_nStateHandle;
        }
        //   SCGSampler& operator=(const SCGSampler& sp)
        //{
        //	this->~SCGSampler();
        //       new(this)SCGSampler(sp);
        //	return *this;
        //}
        //bool operator !=(const SCGSampler& sp) const
        //{
        //	if (sp.m_dwBind == m_dwBind &&
        //		sp.m_Name == m_Name &&
        //		sp.m_nStateHandle == m_nStateHandle &&
        //		sp.m_nParameters == m_nParameters &&
        //		sp.m_eCGSamplerType == m_eCGSamplerType &&
        //		sp.m_dwCBufSlot == m_dwCBufSlot &&
        //		sp.m_Flags == m_Flags)
        //	{
        //		return false;
        //	}
        //	return true;
        //}

        //int Size() => sizeof(SCGSampler);
        //void GetMemoryUsage(ICrySizer pSizer) { }
    }

    public enum ECGTexture : byte
    {
        ECGT_Unknown,

        ECGT_MatSlot_Diffuse,
        ECGT_MatSlot_Normals,
        ECGT_MatSlot_Specular,
        ECGT_MatSlot_Env,
        ECGT_MatSlot_Detail,
        ECGT_MatSlot_Smoothness,
        ECGT_MatSlot_Height,
        ECGT_MatSlot_DecalOverlay,
        ECGT_MatSlot_SubSurface,
        ECGT_MatSlot_Custom,
        ECGT_MatSlot_CustomSecondary,
        ECGT_MatSlot_Opacity,
        ECGT_MatSlot_Translucency,
        ECGT_MatSlot_Emittance,

        ECGT_ScaleformInput0,
        ECGT_ScaleformInput1,
        ECGT_ScaleformInput2,
        ECGT_ScaleformInputY,
        ECGT_ScaleformInputU,
        ECGT_ScaleformInputV,
        ECGT_ScaleformInputA,

        ECGT_Shadow0,
        ECGT_Shadow1,
        ECGT_Shadow2,
        ECGT_Shadow3,
        ECGT_Shadow4,
        ECGT_Shadow5,
        ECGT_Shadow6,
        ECGT_Shadow7,
        ECGT_ShadowMask,

        ECGT_HDR_Target,
        ECGT_HDR_TargetPrev,
        ECGT_HDR_AverageLuminance,
        ECGT_HDR_FinalBloom,

        ECGT_BackBuffer,
        ECGT_BackBufferScaled_d2,
        ECGT_BackBufferScaled_d4,
        ECGT_BackBufferScaled_d8,

        ECGT_ZTarget,
        ECGT_ZTargetMS,
        ECGT_ZTargetScaled_d2,
        ECGT_ZTargetScaled_d4,

        ECGT_SceneTarget,
        ECGT_SceneNormalsBent,
        ECGT_SceneNormals,
        ECGT_SceneDiffuse,
        ECGT_SceneSpecular,
        ECGT_SceneNormalsMS,

        ECGT_VolumetricClipVolumeStencil,
        ECGT_VolumetricFog,
        ECGT_VolumetricFogGlobalEnvProbe0,
        ECGT_VolumetricFogGlobalEnvProbe1,
        ECGT_VolumetricFogShadow0,
        ECGT_VolumetricFogShadow1,

        ECGT_WaterOceanMap,
        ECGT_WaterVolumeDDN,
        ECGT_WaterVolumeCaustics,
        ECGT_WaterVolumeRefl,
        ECGT_RainOcclusion,

        ECGT_TerrainNormMap,
        ECGT_TerrainBaseMap,
        ECGT_TerrainElevMap,

        ECGT_WindGrid,

        ECGT_CloudShadow,
        ECGT_VolCloudShadow,

        ECGT_COUNT
    }

    public class SCGTexture : SCGBind
    {
        CTexture m_pTexture;
        STexAnim m_pAnimInfo;
        ECGTexture m_eCGTextureType = ECGTexture.ECGT_Unknown;
        bool m_bSRGBLookup;
        bool m_bGlobal;

        //SCGTexture() { }
        //   SCGTexture(SCGTexture sp);
        //SCGTexture operator=(SCGTexture sp)
        //{
        //	this->~SCGTexture();
        //       new(this)SCGTexture(sp);
        //	return *this;
        //}
        //   bool operator !=(const SCGTexture& sp) const
        //{
        //	if (sp.m_dwBind == m_dwBind &&
        //		sp.m_Name == m_Name &&
        //		sp.m_nParameters == m_nParameters &&
        //		sp.m_dwCBufSlot == m_dwCBufSlot &&
        //		sp.m_Flags == m_Flags &&
        //		sp.m_pAnimInfo == m_pAnimInfo &&
        //		sp.m_pTexture == m_pTexture &&
        //		sp.m_eCGTextureType == m_eCGTextureType &&
        //		sp.m_bSRGBLookup == m_bSRGBLookup &&
        //		sp.m_bGlobal == m_bGlobal)
        //	{
        //		return false;
        //	}
        //	return true;
        //}
        //int Size() => sizeof(SCGTexture);
        //void GetMemoryUsage(ICrySizer pSizer) { }
    }
}
