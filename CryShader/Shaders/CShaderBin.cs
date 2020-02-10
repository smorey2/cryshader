using System;
using System.Collections.Generic;
using FOURCC = System.UInt32;
using AffectedFuncsVec = System.Collections.Generic.List<int>;
using AffectedParamsVec = System.Collections.Generic.List<int>;
using ParamsCacheVec = System.Collections.Generic.List<CryShader.Shaders.SParamCacheInfo>;
using FXShaderBinValidCRC = System.Collections.Generic.Dictionary<uint, bool>;
using FXShaderBinPath = System.Collections.Generic.Dictionary<string, string>;
using ShaderFXParams = System.Collections.Generic.Dictionary<string, CryShader.Shaders.SShaderFXParams>;
using FXShaderToken = System.Collections.Generic.List<CryShader.Shaders.STokenD>;
using ShaderTokensVec = System.Collections.Generic.List<System.UInt32>;
using System.Diagnostics;
using System.IO;

namespace CryShader.Shaders
{
    struct SShaderTechParseParams
    {
        //string techName[TTYPE_MAX];
    }

    public struct SShaderBinHeader
    {
        public FOURCC m_Magic;
        public uint m_CRC32;
        public ushort m_VersionLow;
        public ushort m_VersionHigh;
        public uint m_nOffsetStringTable;
        public uint m_nOffsetParamsLocal;
        public uint m_nTokens;
        public uint m_nSourceCRC32;
    }

    public struct SShaderBinParamsHeader
    {
        public ulong nMask;
        public uint nName;
        public int nParams;
        public int nSamplers;
        public int nTextures;
        public int nFuncs;
    }

    public class SParamCacheInfo
    {
        public uint m_dwName;
        public ulong m_nMaskGenFX;
        public AffectedFuncsVec m_AffectedFuncs = new AffectedFuncsVec();
        public AffectedParamsVec m_AffectedParams = new AffectedParamsVec();
        public AffectedParamsVec m_AffectedSamplers = new AffectedParamsVec();
        public AffectedParamsVec m_AffectedTextures = new AffectedParamsVec();

        //public int Size()
        //{
        //    return sizeof(SParamCacheInfo) + sizeofVector(m_AffectedFuncs) + sizeofVector(m_AffectedParams) + sizeofVector(m_AffectedSamplers) + sizeofVector(m_AffectedTextures);
        //}

        //public void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    pSizer.AddObject(m_AffectedFuncs);
        //    pSizer.AddObject(m_AffectedParams);
        //    pSizer.AddObject(m_AffectedSamplers);
        //    pSizer.AddObject(m_AffectedTextures);
        //}
    }

    partial class G
    {
        const int MAX_FXBIN_CACHE = 200;
    }

    public class SShaderBin
    {
        public static SShaderBin s_Root;
        public static uint s_nCache;
        public static uint s_nMaxFXBinCache;

        public SShaderBin m_Next;
        public SShaderBin m_Prev;

        public uint m_CRC32;
        public uint m_dwName;
        public string m_szName = string.Empty;
        public uint m_SourceCRC32;
        public bool m_bLocked;
        public bool m_bReadOnly = true;
        public bool m_bInclude;
        public FXShaderToken m_TokenTable;
        public ShaderTokensVec m_Tokens;

        // Local shader info (after parsing)
        public uint m_nOffsetLocalInfo;
        public uint m_nCurCacheParamsID = uint.MaxValue;
        public uint m_nCurParamsID = uint.MaxValue;
        public ParamsCacheVec m_ParamsCache;

        public SShaderBin()
        {
            if (s_Root.m_Next == null)
            {
                s_Root.m_Next = s_Root;
                s_Root.m_Prev = s_Root;
            }
        }

        public void SetName(string name) => m_szName = name;

        public void Unlink()
        {
            if (m_Next == null || m_Prev == null)
                return;
            m_Next.m_Prev = m_Prev;
            m_Prev.m_Next = m_Next;
            m_Next = m_Prev = null;
        }

        void Link(SShaderBin before)
        {
            if (m_Next != null || m_Prev != null)
                return;
            m_Next = before.m_Next;
            before.m_Next.m_Prev = this;
            before.m_Next = this;
            m_Prev = before;
        }
        public bool IsReadOnly() => m_bReadOnly;
        public void Lock() => m_bLocked = true;
        public void Unlock() => m_bLocked = false;

        uint ComputeCRC()
        {
            if (m_Tokens.Count == 0)
                return 0;
            uint CRC32;
            if (CParserBin.m_bEndians)
            {
                //DWORD* pT = new DWORD[m_Tokens.size()];
                //memcpy(pT, m_Tokens, m_Tokens.Count * sizeof(uint));
                //SwapEndian(pT, (size_t)m_Tokens.size(), eBigEndian);
                //CRC32 = CCrc32::Compute((void*)pT, m_Tokens.size() * sizeof(uint32));
                //delete[] pT;
            }
            else
                CRC32 = CCrc32.Compute(m_Tokens, m_Tokens.Count * sizeof(uint));
            int nCur = 0;
            Lock();
            while (nCur >= 0)
            {
                nCur = CParserBin.FindToken(nCur, m_Tokens.Count - 1, m_Tokens, EToken.eT_include);
                if (nCur >= 0)
                {
                    nCur++;
                    var nTokName = m_Tokens[nCur];
                    var szNameInc = CParserBin.GetString(nTokName, m_TokenTable);
                    SShaderBin pBinIncl = gRenDev->m_cEF.m_Bin.GetBinShader(szNameInc, true, 0);
                    Debug.Assert(pBinIncl != null);
                    if (pBinIncl != null)
                        CRC32 += pBinIncl.ComputeCRC();
                }
            }
            Unlock();
            return CRC32;
        }

        void SetCRC(uint nCRC) => m_CRC32 = nCRC;

        void CryptData()
        {
            var pData = m_Tokens;
            var CRC32 = m_CRC32;
            for (var i = 0; i < m_Tokens.Count; i++)
                pData[i] ^= CRC32;
        }

        //int Size()
        //{
        //    int nSize = sizeof(SShaderBin);
        //    nSize += sizeOfV(m_TokenTable);
        //    nSize += sizeofVector(m_Tokens);
        //    nSize += sizeOfV(m_ParamsCache);
        //    return nSize;
        //}

        //void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    pSizer->AddObject(m_TokenTable);
        //    pSizer->AddObject(m_Tokens);
        //    pSizer->AddObject(m_ParamsCache);
        //}

        //private SShaderBin(SShaderBin);
        //private SShaderBin operator=(SShaderBin);
    }

    partial class X
    {
        const int FXP_PARAMS_DIRTY = 1;
        const int FXP_SAMPLERS_DIRTY = 2;
        const int FXP_TEXTURES_DIRTY = 4;
    }

    public class SShaderFXParams
    {
        public uint m_nFlags; // FXP_DIRTY
        public List<SFXParam> m_FXParams = new List<SFXParam>();
        public List<SFXSampler> m_FXSamplers = new List<SFXSampler>();
        public List<SFXTexture> m_FXTextures = new List<SFXTexture>();
        public List<SShaderParam> m_PublicParams;

        //public int Size()
        //{
        //    int nSize = sizeOfV(m_FXParams);
        //    nSize += sizeOfV(m_FXSamplers);
        //    nSize += sizeOfV(m_FXSamplers);
        //    nSize += sizeOfV(m_PublicParams);
        //    return nSize;
        //}
    }

    public class CShaderManBin
    {
        static void sParseCSV(string sFlt, List<string> Filters)
        {
            //          const char* cFlt = sFlt.c_str();
            //          char Flt[64];
            //          int nFlt = 0;
            //          while (true)
            //          {
            //              char c = *cFlt++;
            //              if (!c)
            //                  break;
            //              if (SkipChar((unsigned char)c))
            //{
            //                  if (nFlt)
            //                  {
            //                      Flt[nFlt] = 0;
            //                      Filters.push_back(string(Flt));
            //                      nFlt = 0;
            //                  }
            //                  continue;
            //              }
            //              Flt[nFlt++] = c;
            //          }
            //          if (nFlt)
            //          {
            //              Flt[nFlt] = 0;
            //              Filters.push_back(string(Flt));
            //          }
        }

        //struct FXParamsSortByName
        //{
        //    static bool operator()(SFXParam left, SFXParam right) => left.m_dwName[0] < right.m_dwName[0];
        //    static bool operator()(uint left, SFXParam right) => left<right.m_dwName[0];
        //    static bool operator()(SFXParam left, uint right)  => left.m_dwName[0] < right;
        //}
        //struct FXSamplersSortByName
        //{
        //    static bool operator()(SFXSampler left, SFXSampler right)  => left.m_dwName[0] < right.m_dwName[0];
        //    static bool operator()(uint left, SFXSampler right) => left<right.m_dwName[0]; 
        //    static bool operator()(SFXSampler left, uint right) => left.m_dwName[0] < right;
        //}
        //struct FXTexturesSortByName
        //{
        //    static bool operator()(SFXTexture left, SFXTexture right) => left.m_dwName[0] < right.m_dwName[0];
        //    static bool operator()(uint left, SFXTexture right) => left<right.m_dwName[0];
        //    static bool operator()(SFXTexture left, uint right) => left.m_dwName[0] < right;
        //}

        //=========================================================================================================================================

        private SShaderBin LoadBinShader(FileStream fpBin, string szName, string szNameBin, bool bReadParams)
        {
            //	CRY_PROFILE_FUNCTION(PROFILE_LOADING_ONLY)(iSystem);

            //	gEnv->pCryPak->FSeek(fpBin, 0, SEEK_SET);
            //        SShaderBinHeader Header;
            //        size_t sizeRead = gEnv->pCryPak->FReadRaw(&Header, 1, sizeof(SShaderBinHeader), fpBin);
            //	if (sizeRead != sizeof(SShaderBinHeader))
            //	{
            //		CryWarning(VALIDATOR_MODULE_RENDERER, VALIDATOR_ERROR, "Failed to read header for %s in CShaderManBin::LoadBinShader. Expected %" PRISIZE_T ", got %" PRISIZE_T, szName, sizeof(SShaderBinHeader), sizeRead);
            //		return NULL;
            //	}

            //	if (CParserBin::m_bEndians)
            //        SwapEndian(Header, eBigEndian);
            //    float fVersion = (float)FX_CACHE_VER;
            //    uint16 MinorVer = (uint16)(((float)fVersion - (float)(int)fVersion) * 10.1f);
            //    uint16 MajorVer = (uint16)fVersion;
            //    bool bCheckValid = CRenderer::CV_r_shadersAllowCompilation != 0;
            //	if (bCheckValid && (Header.m_VersionLow != MinorVer || Header.m_VersionHigh != MajorVer || Header.m_Magic != FOURCC_SHADERBIN))
            //		return NULL;
            //	if (Header.m_VersionHigh > 10)
            //		return NULL;
            //	SShaderBin* pBin = new SShaderBin;

            //    pBin->m_SourceCRC32 = Header.m_nSourceCRC32;
            //	pBin->m_nOffsetLocalInfo = Header.m_nOffsetParamsLocal;

            //	uint32 CRC32 = Header.m_CRC32;
            //    pBin->m_CRC32 = CRC32;
            //	pBin->m_Tokens.resize(Header.m_nTokens);
            //	sizeRead = gEnv->pCryPak->FReadRaw(&pBin->m_Tokens[0], sizeof(uint32), Header.m_nTokens, fpBin);
            //	if (sizeRead != Header.m_nTokens)
            //	{
            //		CryWarning(VALIDATOR_MODULE_RENDERER, VALIDATOR_ERROR, "Failed to read Tokens for %s in CShaderManBin::LoadBinShader. Expected %u, got %" PRISIZE_T, szName, Header.m_nTokens, sizeRead);
            //		return NULL;
            //	}
            //	if (CParserBin::m_bEndians)
            //        SwapEndian(&pBin->m_Tokens[0], (size_t) Header.m_nTokens, eBigEndian);

            ////pBin->CryptData();
            //int nSizeTable = Header.m_nOffsetParamsLocal - Header.m_nOffsetStringTable;
            //	if (nSizeTable< 0)
            //		return NULL;
            //	char* bufTable = new char[nSizeTable];
            //char* bufT = bufTable;
            //sizeRead = gEnv->pCryPak->FReadRaw(bufTable, 1, nSizeTable, fpBin);
            //	if (sizeRead != nSizeTable)
            //	{
            //		CryWarning(VALIDATOR_MODULE_RENDERER, VALIDATOR_ERROR, "Failed to read bufTable for %s in CShaderManBin::LoadBinShader. Expected %d, got %" PRISIZE_T, szName, nSizeTable, sizeRead);
            //		return NULL;
            //	}
            //	char* bufEnd = &bufTable[nSizeTable];

            //// First pass to count the tokens
            //uint32 nTokens(0);
            //	while (bufTable<bufEnd)
            //	{
            //		STokenD TD;
            //LoadUnaligned(bufTable, TD.Token);
            //int nIncr = 4 + strlen(&bufTable[4]) + 1;
            //bufTable += nIncr;
            //		++nTokens;
            //	}

            //	pBin->m_TokenTable.reserve(nTokens);
            //	bufTable = bufT;
            //	while (bufTable<bufEnd)
            //	{
            //		STokenD TD;
            //LoadUnaligned(bufTable, TD.Token);
            //		if (CParserBin::m_bEndians)
            //            SwapEndian(TD.Token, eBigEndian);
            //FXShaderTokenItor itor = std::lower_bound(pBin->m_TokenTable.begin(), pBin->m_TokenTable.end(), TD.Token, SortByToken());
            //assert(itor == pBin->m_TokenTable.end() || (* itor).Token != TD.Token);
            //TD.SToken = &bufTable[4];
            //		pBin->m_TokenTable.insert(itor, TD);
            //		int nIncr = 4 + strlen(&bufTable[4]) + 1;
            //bufTable += nIncr;
            //	}
            //	SAFE_DELETE_ARRAY(bufT);

            //	//if (CRenderer::CV_r_shadersnocompile)
            //	//  bReadParams = false;
            //	if (bReadParams)
            //	{
            //		int nSeek = pBin->m_nOffsetLocalInfo;
            //gEnv->pCryPak->FSeek(fpBin, nSeek, SEEK_SET);
            //		while (true)
            //		{
            //			SShaderBinParamsHeader sd;
            //int nSize = gEnv->pCryPak->FReadRaw(&sd, 1, sizeof(sd), fpBin);
            //			if (nSize != sizeof(sd))
            //			{
            //				break;
            //			}
            //			if (CParserBin::m_bEndians)
            //                SwapEndian(sd, eBigEndian);
            //SParamCacheInfo pr;
            //int n = pBin->m_ParamsCache.size();
            //pBin->m_ParamsCache.push_back(pr);
            //			SParamCacheInfo& prc = pBin->m_ParamsCache[n];
            //			prc.m_dwName = sd.nName;
            //			prc.m_nMaskGenFX = sd.nMask;
            //			prc.m_AffectedParams.resize(sd.nParams);
            //			prc.m_AffectedSamplers.resize(sd.nSamplers);
            //			prc.m_AffectedTextures.resize(sd.nTextures);
            //			prc.m_AffectedFuncs.resize(sd.nFuncs);

            //			if (sd.nParams)
            //			{
            //				nSize = gEnv->pCryPak->FReadRaw(&prc.m_AffectedParams[0], sizeof(int32), sd.nParams, fpBin);
            //				if (nSize != sd.nParams)
            //				{
            //					CryWarning(VALIDATOR_MODULE_RENDERER, VALIDATOR_ERROR, "Failed to read m_AffectedParams for %s in CShaderManBin::LoadBinShader. Expected %d, got %" PRISIZE_T, szName, sd.nParams, sizeRead);
            //					return NULL;
            //				}
            //				if (CParserBin::m_bEndians)
            //                    SwapEndian(&prc.m_AffectedParams[0], sd.nParams, eBigEndian);
            //			}
            //			if (sd.nSamplers)
            //			{
            //				nSize = gEnv->pCryPak->FReadRaw(&prc.m_AffectedSamplers[0], sizeof(int32), sd.nSamplers, fpBin);
            //				if (nSize != sd.nSamplers)
            //				{
            //					CryWarning(VALIDATOR_MODULE_RENDERER, VALIDATOR_ERROR, "Failed to read m_AffectedSamplers for %s in CShaderManBin::LoadBinShader. Expected %d, got %" PRISIZE_T, szName, sd.nSamplers, sizeRead);
            //					return NULL;
            //				}
            //				if (CParserBin::m_bEndians)
            //                    SwapEndian(&prc.m_AffectedSamplers[0], sd.nSamplers, eBigEndian);
            //			}
            //			if (sd.nTextures)
            //			{
            //				nSize = gEnv->pCryPak->FReadRaw(&prc.m_AffectedTextures[0], sizeof(int32), sd.nTextures, fpBin);
            //				if (nSize != sd.nTextures)
            //				{
            //					CryWarning(VALIDATOR_MODULE_RENDERER, VALIDATOR_ERROR, "Failed to read m_AffectedTextures for %s in CShaderManBin::LoadBinShader. Expected %d, got %" PRISIZE_T, szName, sd.nTextures, sizeRead);
            //					return NULL;
            //				}
            //				if (CParserBin::m_bEndians)
            //                    SwapEndian(&prc.m_AffectedTextures[0], sd.nTextures, eBigEndian);
            //			}

            //			assert(sd.nFuncs > 0);
            //nSize = gEnv->pCryPak->FReadRaw(&prc.m_AffectedFuncs[0], sizeof(int32), sd.nFuncs, fpBin);
            //			if (nSize != sd.nFuncs)
            //			{
            //				CryWarning(VALIDATOR_MODULE_RENDERER, VALIDATOR_ERROR, "Failed to read nFuncs for %s in CShaderManBin::LoadBinShader. Expected %d, got %" PRISIZE_T, szName, sd.nFuncs, sizeRead);
            //				return NULL;
            //			}
            //			if (CParserBin::m_bEndians)
            //                SwapEndian(&prc.m_AffectedFuncs[0], sd.nFuncs, eBigEndian);

            //nSeek += (sd.nFuncs) * sizeof(int32) + sizeof(sd);
            //		}
            //	}

            //	char nameLwr[256];
            //cry_strcpy(nameLwr, szName);
            //strlwr(nameLwr);
            //pBin->SetName(szNameBin);
            //pBin->m_dwName = CParserBin::GetCRC32(nameLwr);

            //	return pBin;
        }
        private SShaderBin SaveBinShader(uint nSourceCRC32, string szName, bool bInclude, FileStream fpSrc)
        {

            var pBin = new SShaderBin();

            /*
                        var Parser = new CParserBin(pBin);

                        gEnv.pCryPak.FSeek(fpSrc, 0, SEEK_END);
                        int nSize = gEnv.pCryPak.FTell(fpSrc);
                        var buf = new char[nSize + 1];
                        var pBuf = buf;
                        buf[nSize] = 0;
                        gEnv.pCryPak.FSeek(fpSrc, 0, SEEK_SET);
                        gEnv.pCryPak.FRead(buf, nSize, fpSrc);

                        RemoveCR(buf);
                        string kWhiteSpace = " ";

                        while (buf != null && buf[0] != null)
                        {
                            SkipCharacters(&buf, kWhiteSpace);
                            SkipComments(&buf, true);
                            if (!buf || !buf[0])
                                break;

                            char com[1024];
                            bool bKey = false;
                            uint32 dwToken = CParserBin::NextToken(buf, com, bKey);
                            dwToken = Parser.NewUserToken(dwToken, com, false);
                            pBin->m_Tokens.push_back(dwToken);

                            SkipCharacters(&buf, kWhiteSpace);
                            SkipComments(&buf, true);
                            if (dwToken >= eT_float && dwToken <= eT_int)
                            {

                            }

                            if (dwToken == eT_include)
                            {
                                assert(bKey);
                                SkipCharacters(&buf, kWhiteSpace);
                                assert(*buf == '"' || *buf == '<');
                                char brak = *buf;
                                ++buf;
                                int n = 0;
                                while (*buf != brak)
                                {
                                    if (*buf <= 0x20)
                                    {
                                        assert(0);
                                        break;
                                    }
                                    com[n++] = *buf;
                                    ++buf;
                                }
                                if (*buf == brak)
                                    ++buf;
                                com[n] = 0;

                                PathUtil::RemoveExtension(com);

                                //SShaderBin* pBIncl = GetBinShader(com, true, 0);
                                //
                                //assert(pBIncl);

                                dwToken = CParserBin::fxToken(com, NULL);
                                dwToken = Parser.NewUserToken(dwToken, com, false);
                                pBin->m_Tokens.push_back(dwToken);
                            }
                            else if (dwToken == eT_if || dwToken == eT_ifdef || dwToken == eT_ifndef || dwToken == eT_elif)
                            {
                                bool bFirst = fxIsFirstPass(buf);
                                if (!bFirst)
                                {
                                    if (dwToken == eT_if)
                                        dwToken = eT_if_2;
                                    else if (dwToken == eT_ifdef)
                                        dwToken = eT_ifdef_2;
                                    else if (dwToken == eT_elif)
                                        dwToken = eT_elif_2;
                                    else
                                        dwToken = eT_ifndef_2;
                                    pBin->m_Tokens[pBin->m_Tokens.size() - 1] = dwToken;
                                }
                            }
                            else if (dwToken == eT_define)
                            {
                                shFill(&buf, com);
                                if (com[0] == '%')
                                    pBin->m_Tokens[pBin->m_Tokens.size() - 1] = eT_define_2;
                                dwToken = Parser.NewUserToken(eT_unknown, com, false);
                                pBin->m_Tokens.push_back(dwToken);

                                TArray<char> macro;
                                while (*buf == 0x20 || *buf == 0x9)
                                {
                                    buf++;
                                }
                                while (*buf != 0xa)
                                {
                                    if (*buf == '\\')
                                    {
                                        macro.AddElem('\n');
                                        while (*buf != '\n')
                                        {
                                            buf++;
                                        }
                                        buf++;
                                        continue;
                                    }
                                    macro.AddElem(*buf);
                                    buf++;
                                }
                                macro.AddElem(0);
                                int n = macro.Num() - 2;
                                while (n >= 0 && macro[n] <= 0x20)
                                {
                                    macro[n] = 0;
                                    n--;
                                }
                                const char* b = &macro[0];
                                while (*b)
                                {
                                    SkipCharacters(&b, kWhiteSpace);
                                    SkipComments((char**)&b, true);
                                    if (!b[0])
                                        break;
                                    bKey = false;
                                    dwToken = CParserBin::NextToken(b, com, bKey);
                                    dwToken = Parser.NewUserToken(dwToken, com, false);
                                    if (dwToken == eT_if || dwToken == eT_ifdef || dwToken == eT_ifndef || dwToken == eT_elif)
                                    {
                                        bool bFirst = fxIsFirstPass(b);
                                        if (!bFirst)
                                        {
                                            if (dwToken == eT_if)
                                                dwToken = eT_if_2;
                                            else if (dwToken == eT_ifdef)
                                                dwToken = eT_ifdef_2;
                                            else if (dwToken == eT_elif)
                                                dwToken = eT_elif_2;
                                            else
                                                dwToken = eT_ifndef_2;
                                        }
                                    }
                                    pBin->m_Tokens.push_back(dwToken);
                                }
                                pBin->m_Tokens.push_back(0);
                            }
                        }
                        if (!pBin->m_Tokens.size() || !pBin->m_Tokens[0])
                            pBin->m_Tokens.push_back(eT_skip);

                        pBin->SetCRC(pBin->ComputeCRC());
                        pBin->m_bReadOnly = false;
                        //pBin->CryptData();
                        //pBin->CryptTable();

                        char nameFile[256];
                        cry_sprintf(nameFile, "%s%s.%s", m_pCEF->m_ShadersCache, szName, bInclude ? "cfib" : "cfxb");
                        stack_string szDst = stack_string(m_pCEF->m_szUserPath.c_str()) + stack_string(nameFile);
                        const char* szFileName = szDst;

                        FILE* fpDst = gEnv->pCryPak->FOpen(szFileName, "wb", ICryPak::FLAGS_NEVER_IN_PAK | ICryPak::FLAGS_PATH_REAL | ICryPak::FOPEN_ONDISK);
                        if (fpDst)
                        {
                            SShaderBinHeader Header;
                            Header.m_nTokens = pBin->m_Tokens.size();
                            Header.m_Magic = FOURCC_SHADERBIN;
                            Header.m_CRC32 = pBin->m_CRC32;
                            float fVersion = (float)FX_CACHE_VER;
                            Header.m_VersionLow = (uint16)(((float)fVersion - (float)(int)fVersion) * 10.1f);
                            Header.m_VersionHigh = (uint16)fVersion;
                            Header.m_nOffsetStringTable = pBin->m_Tokens.size() * sizeof(DWORD) + sizeof(Header);
                            Header.m_nOffsetParamsLocal = 0;
                            Header.m_nSourceCRC32 = nSourceCRC32;
                            SShaderBinHeader hdTemp, *pHD;
                            pHD = &Header;
                            if (CParserBin::m_bEndians)
                            {
                                hdTemp = Header;
                                SwapEndian(hdTemp, eBigEndian);
                                pHD = &hdTemp;
                            }
                            gEnv->pCryPak->FWrite((void*)pHD, sizeof(Header), 1, fpDst);
                            if (CParserBin::m_bEndians)
                            {
                                DWORD* pT = new DWORD[pBin->m_Tokens.size()];
                                memcpy(pT, &pBin->m_Tokens[0], pBin->m_Tokens.size() * sizeof(DWORD));
                                SwapEndian(pT, (size_t)pBin->m_Tokens.size(), eBigEndian);
                                gEnv->pCryPak->FWrite((void*)pT, pBin->m_Tokens.size() * sizeof(DWORD), 1, fpDst);
                                delete[] pT;
                            }
                            else
                                gEnv->pCryPak->FWrite(&pBin->m_Tokens[0], pBin->m_Tokens.size() * sizeof(DWORD), 1, fpDst);
                            FXShaderTokenItor itor;
                            for (itor = pBin->m_TokenTable.begin(); itor != pBin->m_TokenTable.end(); itor++)
                            {
                                STokenD T = *itor;
                                if (CParserBin::m_bEndians)
                                    SwapEndian(T.Token, eBigEndian);
                                gEnv->pCryPak->FWrite(&T.Token, sizeof(DWORD), 1, fpDst);
                                gEnv->pCryPak->FWrite(T.SToken.c_str(), T.SToken.size() + 1, 1, fpDst);
                            }
                            Header.m_nOffsetParamsLocal = gEnv->pCryPak->FTell(fpDst);
                            gEnv->pCryPak->FSeek(fpDst, 0, SEEK_SET);
                            if (CParserBin::m_bEndians)
                            {
                                hdTemp = Header;
                                SwapEndian(hdTemp, eBigEndian);
                                pHD = &hdTemp;
                            }
                            gEnv->pCryPak->FWrite((void*)pHD, sizeof(Header), 1, fpDst);
                            gEnv->pCryPak->FClose(fpDst);
                        }
                        else
                        {
                            iLog->LogWarning("CShaderManBin::SaveBinShader: Cannot write shader to file '%s'.", nameFile);
                            pBin->m_bReadOnly = true;
                        }

                        SAFE_DELETE_ARRAY(pBuf);

                */
            return pBin;
        }
        private bool SaveBinShaderLocalInfo(SShaderBin pBin, uint dwName, ulong nMaskGenFX, int[] Funcs, List<SFXParam> Params, List<SFXSampler> Samplers, List<SFXTexture> Textures)
        {
            //if (GetParamInfo(pBin, dwName, nMaskGenFX))
            //    return true;
            ////return false;
            //if (pBin->IsReadOnly() && !gEnv->IsEditor()) // if in the editor, allow params to be added in-memory, but not saved to disk
            //    return false;
            //TArray<int32> EParams;
            //TArray<int32> ESamplers;
            //TArray<int32> ETextures;
            //TArray<int32> EFuncs;
            //for (uint32 i = 0; i < Params.size(); i++)
            //{
            //    SFXParam & pr = Params[i];
            //    assert(pr.m_dwName.size());
            //    if (pr.m_dwName.size())
            //        EParams.push_back(pr.m_dwName[0]);
            //}
            //for (uint32 i = 0; i < Samplers.size(); i++)
            //{
            //    SFXSampler & pr = Samplers[i];
            //    assert(pr.m_dwName.size());
            //    if (pr.m_dwName.size())
            //        ESamplers.push_back(pr.m_dwName[0]);
            //}
            //for (uint32 i = 0; i < Textures.size(); i++)
            //{
            //    SFXTexture & pr = Textures[i];
            //    assert(pr.m_dwName.size());
            //    if (pr.m_dwName.size())
            //        ETextures.push_back(pr.m_dwName[0]);
            //}
            //pBin->m_nCurParamsID = pBin->m_ParamsCache.size();
            //pBin->m_ParamsCache.push_back(SParamCacheInfo());
            //SParamCacheInfo & pr = pBin->m_ParamsCache.back();
            //pr.m_nMaskGenFX = nMaskGenFX;
            //pr.m_dwName = dwName;
            //pr.m_AffectedFuncs.assign(Funcs.begin(), Funcs.end());
            //pr.m_AffectedParams.assign(EParams.begin(), EParams.end());
            //pr.m_AffectedSamplers.assign(ESamplers.begin(), ESamplers.end());
            //pr.m_AffectedTextures.assign(ETextures.begin(), ETextures.end());
            //if (pBin->IsReadOnly())
            //    return false;
            //FILE* fpBin = gEnv->pCryPak->FOpen(pBin->m_szName, "r+b", ICryPak::FLAGS_NEVER_IN_PAK | ICryPak::FLAGS_PATH_REAL | ICryPak::FOPEN_ONDISK);
            //assert(fpBin);
            //if (!fpBin)
            //    return false;
            //gEnv->pCryPak->FSeek(fpBin, 0, SEEK_END);
            //int nSeek = gEnv->pCryPak->FTell(fpBin);
            //assert(nSeek > 0);
            //if (nSeek <= 0)
            //    return false;
            //SShaderBinParamsHeader sd;
            //int32* pFuncs = &Funcs[0];
            //int32* pParams = EParams.size() ? &EParams[0] : NULL;
            //int32* pSamplers = ESamplers.size() ? &ESamplers[0] : NULL;
            //int32* pTextures = ETextures.size() ? &ETextures[0] : NULL;
            //sd.nMask = nMaskGenFX;
            //sd.nName = dwName;
            //sd.nFuncs = Funcs.size();
            //sd.nParams = EParams.size();
            //sd.nSamplers = ESamplers.size();
            //sd.nTextures = ETextures.size();
            //if (CParserBin::m_bEndians)
            //{
            //    SwapEndian(sd, eBigEndian);
            //    EFuncs = Funcs;
            //    if (EParams.size())
            //        SwapEndian(&EParams[0], (size_t)EParams.size(), eBigEndian);
            //    if (ESamplers.size())
            //        SwapEndian(&ESamplers[0], (size_t)ESamplers.size(), eBigEndian);
            //    if (ETextures.size())
            //        SwapEndian(&ETextures[0], (size_t)ETextures.size(), eBigEndian);
            //    SwapEndian(&EFuncs[0], (size_t)EFuncs.size(), eBigEndian);
            //    pFuncs = &EFuncs[0];
            //}
            //gEnv->pCryPak->FWrite(&sd, sizeof(sd), 1, fpBin);
            //if (EParams.size())
            //    gEnv->pCryPak->FWrite(pParams, EParams.size(), sizeof(int32), fpBin);
            //if (ESamplers.size())
            //    gEnv->pCryPak->FWrite(pSamplers, ESamplers.size(), sizeof(int32), fpBin);
            //if (ETextures.size())
            //    gEnv->pCryPak->FWrite(pTextures, ETextures.size(), sizeof(int32), fpBin);
            //gEnv->pCryPak->FWrite(pFuncs, Funcs.size(), sizeof(int32), fpBin);
            //gEnv->pCryPak->FClose(fpBin);

            //return true;
        }

        private SParamCacheInfo GetParamInfo(SShaderBin pBin, uint dwName, ulong nMaskGenFX)
        {
            //const int n = pBin->m_ParamsCache.size();
            //for (int i = 0; i < n; i++)
            //{
            //    SParamCacheInfo* pInf = &pBin->m_ParamsCache[i];
            //    if (pInf->m_dwName == dwName && pInf->m_nMaskGenFX == nMaskGenFX)
            //    {
            //        pBin->m_nCurParamsID = i;
            //        return pInf;
            //    }
            //}
            //pBin->m_nCurParamsID = -1;
            return null;
        }

        private bool ParseBinFX_Global_Annotations(CParserBin Parser, SParserFrame Frame, bool bPublic, string[] techStart)
        {
            bool bRes = true;

            SParserFrame OldFrame = Parser.BeginFrame(Frame);

            FX_BEGIN_TOKENS;
            FX_TOKEN(ShaderType);
            FX_TOKEN(ShaderDrawType);
            FX_TOKEN(PreprType);
            FX_TOKEN(Public);
            FX_TOKEN(NoPreview);
            FX_TOKEN(LocalConstants);
            FX_TOKEN(Cull);
            FX_TOKEN(SupportsAttrInstancing);
            FX_TOKEN(SupportsConstInstancing);
            FX_TOKEN(SupportsDeferredShading);
            FX_TOKEN(SupportsFullDeferredShading);
            FX_TOKEN(Decal);
            FX_TOKEN(DecalNoDepthOffset);
            FX_TOKEN(Sky);
            FX_TOKEN(HWTessellation);
            FX_TOKEN(ZPrePass);
            FX_TOKEN(VertexColors);
            FX_TOKEN(NoChunkMerging);
            FX_TOKEN(ForceTransPass);
            FX_TOKEN(AfterHDRPostProcess);
            FX_TOKEN(AfterPostProcess);
            FX_TOKEN(ForceZpass);
            FX_TOKEN(ForceWaterPass);
            FX_TOKEN(ForceDrawLast);
            FX_TOKEN(ForceDrawFirst);
            FX_TOKEN(Hair);
            FX_TOKEN(ForceGeneralPass);
            FX_TOKEN(ForceDrawAfterWater);
            FX_TOKEN(DepthFixup);
            FX_TOKEN(DepthFixupReplace);
            FX_TOKEN(SingleLightPass);
            FX_TOKEN(Refractive);
            FX_TOKEN(ForceRefractionUpdate);
            FX_TOKEN(WaterParticle);
            FX_TOKEN(VT_DetailBending);
            FX_TOKEN(VT_DetailBendingGrass);
            FX_TOKEN(VT_WindBending);
            FX_TOKEN(AlphaBlendShadows);
            FX_TOKEN(EyeOverlay);
            FX_TOKEN(WrinkleBlending);
            FX_TOKEN(Billboard);
            FX_END_TOKENS;

            //int nIndex;
            //CShader* ef = Parser.m_pCurShader;

            //while (Parser.ParseObject(sCommands, nIndex))
            //{
            //    EToken eT = Parser.GetToken();
            //    switch (eT)
            //    {
            //        case eT_Public:
            //            if (bPublic)
            //                *bPublic = true;
            //            break;

            //        case eT_NoPreview:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_NOPREVIEW;
            //            break;

            //        case eT_Decal:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_DECAL;
            //            ef->m_nMDV |= MDV_DEPTH_OFFSET;
            //            break;

            //        case eT_DecalNoDepthOffset:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_DECAL;
            //            break;

            //        case eT_Sky:
            //            if (!ef)
            //                break;
            //            break;

            //        case eT_LocalConstants:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_LOCALCONSTANTS;
            //            break;

            //        case eT_VT_DetailBending:
            //            if (!ef)
            //                break;
            //            ef->m_nMDV |= MDV_DET_BENDING;
            //            break;
            //        case eT_VT_DetailBendingGrass:
            //            if (!ef)
            //                break;
            //            ef->m_nMDV |= MDV_DET_BENDING | MDV_DET_BENDING_GRASS;
            //            break;
            //        case eT_VT_WindBending:
            //            if (!ef)
            //                break;
            //            ef->m_nMDV |= MDV_WIND;
            //            break;

            //        case eT_NoChunkMerging:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_NOCHUNKMERGING;
            //            break;

            //        case eT_SupportsAttrInstancing:
            //            if (!ef)
            //                break;
            //            if (gRenDev->m_bDeviceSupportsInstancing)
            //                ef->m_Flags |= EF_SUPPORTSINSTANCING_ATTR;
            //            break;
            //        case eT_SupportsConstInstancing:
            //            if (!ef)
            //                break;
            //            if (gRenDev->m_bDeviceSupportsInstancing)
            //                ef->m_Flags |= EF_SUPPORTSINSTANCING_CONST;
            //            break;

            //        case eT_SupportsDeferredShading:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_SUPPORTSDEFERREDSHADING_MIXED;
            //            break;

            //        case eT_SupportsFullDeferredShading:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_SUPPORTSDEFERREDSHADING_FULL;
            //            break;

            //        case eT_ForceTransPass:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_FORCE_TRANSPASS;
            //            break;

            //        case eT_AfterHDRPostProcess:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_AFTERHDRPOSTPROCESS;
            //            break;

            //        case eT_AfterPostProcess:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_AFTERPOSTPROCESS;
            //            break;

            //        case eT_ForceZpass:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_FORCE_ZPASS;
            //            break;

            //        case eT_ForceWaterPass:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_FORCE_WATERPASS;
            //            break;

            //        case eT_ForceDrawLast:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_FORCE_DRAWLAST;
            //            break;
            //        case eT_ForceDrawFirst:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_FORCE_DRAWFIRST;
            //            break;

            //        case eT_Hair:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_HAIR;
            //            break;

            //        case eT_ForceGeneralPass:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_FORCE_GENERALPASS;
            //            break;

            //        case eT_ForceDrawAfterWater:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_FORCE_DRAWAFTERWATER;
            //            break;
            //        case eT_DepthFixup:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_DEPTH_FIXUP;
            //            break;
            //        case eT_DepthFixupReplace:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_DEPTH_FIXUP_REPLACE;
            //            break;
            //        case eT_SingleLightPass:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_SINGLELIGHTPASS;
            //            break;
            //        case eT_WaterParticle:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_WATERPARTICLE;
            //            break;
            //        case eT_Refractive:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_REFRACTIVE;
            //            break;
            //        case eT_ForceRefractionUpdate:
            //            if (!ef)
            //                break;
            //            ef->m_Flags |= EF_FORCEREFRACTIONUPDATE;
            //            break;

            //        case eT_ZPrePass:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_ZPREPASS;
            //            break;

            //        case eT_HWTessellation:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_HW_TESSELLATION;
            //            break;

            //        case eT_AlphaBlendShadows:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_ALPHABLENDSHADOWS;
            //            break;

            //        case eT_EyeOverlay:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_EYE_OVERLAY;
            //            break;

            //        case eT_VertexColors:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_VERTEXCOLORS;
            //            break;

            //        case eT_Billboard:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_BILLBOARD;
            //            break;

            //        case eT_WrinkleBlending:
            //            if (!ef)
            //                break;
            //            ef->m_Flags2 |= EF2_WRINKLE_BLENDING;
            //            break;

            //        case eT_ShaderDrawType:
            //            {
            //                if (!ef)
            //                    break;
            //                eT = Parser.GetToken(Parser.m_Data);
            //                if (eT == eT_Light)
            //                    ef->m_eSHDType = eSHDT_Light;
            //                else if (eT == eT_Shadow)
            //                    ef->m_eSHDType = eSHDT_Shadow;
            //                else if (eT == eT_Fur)
            //                    ef->m_eSHDType = eSHDT_Fur;
            //                else if (eT == eT_General)
            //                    ef->m_eSHDType = eSHDT_General;
            //                else if (eT == eT_Terrain)
            //                    ef->m_eSHDType = eSHDT_Terrain;
            //                else if (eT == eT_Overlay)
            //                    ef->m_eSHDType = eSHDT_Overlay;
            //                else if (eT == eT_NoDraw)
            //                {
            //                    ef->m_eSHDType = eSHDT_NoDraw;
            //                    ef->m_Flags |= EF_NODRAW;
            //                }
            //                else if (eT == eT_Custom)
            //                    ef->m_eSHDType = eSHDT_CustomDraw;
            //                else if (eT == eT_Sky)
            //                    ef->m_eSHDType = eSHDT_Sky;
            //                else if (eT == eT_OceanShore)
            //                    ef->m_eSHDType = eSHDT_OceanShore;
            //                else if (eT == eT_DebugHelper)
            //                    ef->m_eSHDType = eSHDT_DebugHelper;
            //                else
            //                {
            //                    Warning("Unknown shader draw type '%s'", Parser.GetString(eT));
            //                    assert(0);
            //                }
            //            }
            //            break;

            //        case eT_ShaderType:
            //            {
            //                if (!ef)
            //                    break;
            //                eT = Parser.GetToken(Parser.m_Data);
            //                if (eT == eT_General)
            //                    ef->m_eShaderType = eST_General;
            //                else if (eT == eT_Metal)
            //                    ef->m_eShaderType = eST_Metal;
            //                else if (eT == eT_Ice)
            //                    ef->m_eShaderType = eST_Ice;
            //                else if (eT == eT_Shadow)
            //                    ef->m_eShaderType = eST_Shadow;
            //                else if (eT == eT_Water)
            //                    ef->m_eShaderType = eST_Water;
            //                else if (eT == eT_FX)
            //                    ef->m_eShaderType = eST_FX;
            //                else if (eT == eT_HUD3D)
            //                    ef->m_eShaderType = eST_HUD3D;
            //                else if (eT == eT_PostProcess)
            //                    ef->m_eShaderType = eST_PostProcess;
            //                else if (eT == eT_HDR)
            //                    ef->m_eShaderType = eST_HDR;
            //                else if (eT == eT_Glass)
            //                    ef->m_eShaderType = eST_Glass;
            //                else if (eT == eT_Vegetation)
            //                    ef->m_eShaderType = eST_Vegetation;
            //                else if (eT == eT_Particle)
            //                    ef->m_eShaderType = eST_Particle;
            //                else if (eT == eT_Terrain)
            //                    ef->m_eShaderType = eST_Terrain;
            //                else if (eT == eT_Compute)
            //                    ef->m_eShaderType = eST_Compute;
            //                else
            //                {
            //                    Warning("Unknown shader type '%s'", Parser.GetString(eT));
            //                    assert(0);
            //                }
            //            }
            //            break;

            //        case eT_PreprType:
            //            {
            //                if (!ef)
            //                    break;
            //                eT = Parser.GetToken(Parser.m_Data);
            //                if (eT == eT_ScanWater)
            //                    ef->m_Flags2 |= EF2_PREPR_SCANWATER;
            //                else
            //                {
            //                    Warning("Unknown preprocess type '%s'", Parser.GetString(eT));
            //                    assert(0);
            //                }
            //            }
            //            break;

            //        case eT_Cull:
            //            {
            //                if (!ef)
            //                    break;
            //                eT = Parser.GetToken(Parser.m_Data);
            //                if (eT == eT_None || eT == eT_NONE)
            //                    ef->m_eCull = eCULL_None;
            //                else if (eT == eT_CCW || eT == eT_Back)
            //                    ef->m_eCull = eCULL_Back;
            //                else if (eT == eT_CW || eT == eT_Front)
            //                    ef->m_eCull = eCULL_Front;
            //                else
            //                    assert(0);
            //            }
            //            break;

            //        default:
            //            assert(0);
            //    }
            //}

            //Parser.EndFrame(OldFrame);

            return bRes;
        }

        //private bool ParseBinFX_Global(CParserBin Parser, SParserFrame Frame, bool bPublic, CCryNameR[] techStart);
        //private bool ParseBinFX_Texture_Annotations_Script(CParserBin Parser, SParserFrame Frame, SFXTexture pTexture);
        //private bool ParseBinFX_Texture_Annotations(CParserBin Parser, SParserFrame Frame, SFXTexture pTexture);
        //private bool ParseBinFX_Texture(CParserBin Parser, SParserFrame Data, SFXTexture Sampl, SParserFrame Annotations);

        //private void InitShaderDependenciesList(CParserBin Parser, SCodeFragment pFunc, byte[] bChecked, int[] AffectedFuncs);
        //private void CheckFragmentsDependencies(CParserBin Parser, byte[] bChecked, int[] AffectedFuncs);
        //private void CheckStructuresDependencies(CParserBin Parser, SCodeFragment pFrag, byte[] bChecked, int[] AffectedFunc);

        //private void AddParameterToScript(CParserBin Parser, SFXParam pr, uint[] SHData, EHWShaderClass eSHClass, int nCB);
        //private void AddSamplerToScript(CParserBin Parser, SFXSampler pr, uint[] SHData, EHWShaderClass eSHClass);
        //private void AddTextureToScript(CParserBin Parser, SFXTexture pr, uint[] SHData, EHWShaderClass eSHClass);
        //private void AddAffectedParameter(CParserBin Parser, List<SFXParam> AffectedParams, int[] AffectedFunc, SFXParam pParam, EHWShaderClass eSHClass, uint dwType, SShaderTechnique pShTech);
        //private void AddAffectedSampler(CParserBin Parser, List<SFXSampler> AffectedSamplers, int[] AffectedFunc, SFXSampler pParam, EHWShaderClass eSHClass, uint dwType, SShaderTechnique pShTech);
        //private void AddAffectedTexture(CParserBin Parser, List<SFXTexture> AffectedTextures, int[] AffectedFunc, SFXTexture pParam, EHWShaderClass eSHClass, uint dwType, SShaderTechnique pShTech);
        //private bool ParseBinFX_Technique_Pass_PackParameters(CParserBin Parser, List<SFXParam> AffectedParams, int[] AffectedFunc, SCodeFragment pFunc, EHWShaderClass eSHClass, uint dwSHName, List<SFXParam> PackedParams, SCodeFragment[] Replaces, uint[] NewTokens, byte[] bMerged);
        //private bool ParseBinFX_Technique_Pass_GenerateShaderData(CParserBin Parser, FXMacroBin Macros, SShaderFXParams FXParams, uint dwSHName, EHWShaderClass eSHClass, ulong nGenMask, uint dwSHType, uint[] SHData, SShaderTechnique pShTech);
        //private bool ParseBinFX_Technique_Pass_LoadShader(CParserBin Parser, FXMacroBin Macros, SParserFrame SHFrame, SShaderTechnique pShTech, SShaderPass pPass, EHWShaderClass eSHClass, SShaderFXParams FXParams);
        //private bool ParseBinFX_Technique_Pass(CParserBin Parser, SParserFrame Frame, SShaderTechnique pTech);
        //private bool ParseBinFX_Technique_Annotations_String(CParserBin Parser, SParserFrame Frame, SShaderTechnique pSHTech, List<SShaderTechParseParams> techParams, bool bPublic);
        //private bool ParseBinFX_Technique_Annotations(CParserBin Parser, SParserFrame Frame, SShaderTechnique pSHTech, List<SShaderTechParseParams> techParams, bool bPublic);
        //private bool ParseBinFX_Technique_CustomRE(CParserBin Parser, SParserFrame Frame, SParserFrame Name, SShaderTechnique pShTech);
        //private SShaderTechnique ParseBinFX_Technique(CParserBin Parser, SParserFrame Data, SParserFrame Annotations, List<SShaderTechParseParams> techParams, bool bPublic);
        //private bool ParseBinFX_LightStyle_Val(CParserBin Parser, SParserFrame Frame, CLightStyle ls);
        //private bool ParseBinFX_LightStyle(CParserBin Parser, SParserFrame Frame, int nStyle);

        //private void MergeTextureSlots(SShaderTexSlots master, SShaderTexSlots overlay);
        //private SShaderTexSlots GetTextureSlots(CParserBin Parser, SShaderBin pBin, CShader ef, int nTech = 0, int nPass = 0);

        private SShaderBin SearchInCache(string szName, bool bInclude)
        {
            //char nameFile[256], nameLwr[256];
            //cry_strcpy(nameLwr, szName);
            //strlwr(nameLwr);
            //cry_sprintf(nameFile, "%s.%s", nameLwr, bInclude ? "cfi" : "cfx");
            //uint32 dwName = CParserBin::GetCRC32(nameFile);

            //SShaderBin* pSB;
            //for (pSB = SShaderBin::s_Root.m_Prev; pSB != &SShaderBin::s_Root; pSB = pSB->m_Prev)
            //{
            //    if (pSB->m_dwName == dwName)
            //    {
            //        pSB->Unlink();
            //        pSB->Link(&SShaderBin::s_Root);
            //        return pSB;
            //    }
            //}
            return null;
        }

        private bool AddToCache(SShaderBin pSB, bool bInclude)
        {
            //if (!CRenderer::CV_r_shadersediting)
            //{
            //    if (SShaderBin::s_nCache > SShaderBin::s_nMaxFXBinCache)
            //    {
            //        SShaderBin* pS;
            //        for (pS = SShaderBin::s_Root.m_Prev; pS != &SShaderBin::s_Root; pS = pS->m_Prev)
            //        {
            //            if (!pS->m_bLocked)
            //            {
            //                DeleteFromCache(pS);
            //                break;
            //            }
            //        }
            //    }
            //    assert(SShaderBin::s_nCache <= SShaderBin::s_nMaxFXBinCache);
            //}

            //pSB->m_bInclude = bInclude;
            //pSB->Link(&SShaderBin::s_Root);
            //SShaderBin::s_nCache++;

            return true;
        }

        private bool DeleteFromCache(SShaderBin pSB)
        {
            //assert(pSB != &SShaderBin::s_Root);
            //pSB->Unlink();
            //SAFE_DELETE(pSB);
            //SShaderBin::s_nCache--;

            return true;
        }

        private SFXParam mfAddFXParam(SShaderFXParams FXP, SFXParam pParam)
        {
            //FXParamsIt it = std::lower_bound(FXP.m_FXParams.begin(), FXP.m_FXParams.end(), pParam->m_dwName[0], FXParamsSortByName());
            //if (it != FXP.m_FXParams.end() && pParam->m_dwName[0] == (*it).m_dwName[0])
            //{
            //    SFXParam & pr = *it;
            //    pr.m_nFlags = pParam->m_nFlags;
            //    int n = 6;
            //    SFXParam & p = *it;
            //    for (int i = 0; i < n; i++)
            //    {
            //        if (p.m_nRegister[i] == 10000)
            //            p.m_nRegister[i] = pParam->m_nRegister[i];
            //    }
            //    //assert(p == *pParam);
            //    return &(*it);
            //}
            //FXP.m_FXParams.insert(it, *pParam);
            //it = std::lower_bound(FXP.m_FXParams.begin(), FXP.m_FXParams.end(), pParam->m_dwName[0], FXParamsSortByName());
            //SFXParam* pFX = &(*it);
            //if (pFX->m_Semantic.empty() && pFX->m_Values.c_str()[0] == '(')
            //    pFX->m_nCB = CB_PER_MATERIAL;
            //FXP.m_nFlags |= FXP_PARAMS_DIRTY;

            return pFX;
        }

        private SFXParam mfAddFXParam(CShader pSH, SFXParam pParam)
        {
            //if (!pParam)
            //    return null;
            //SShaderFXParams & FXP = mfGetFXParams(pSH);
            //return mfAddFXParam(FXP, pParam);
        }

        private void mfAddFXSampler(CShader pSH, SFXSampler pParam)
        {
            //if (!pSamp)
            //    return;

            //SShaderFXParams & FXP = mfGetFXParams(pSH);

            //FXSamplersIt it = std::lower_bound(FXP.m_FXSamplers.begin(), FXP.m_FXSamplers.end(), pSamp->m_dwName[0], FXSamplersSortByName());
            //if (it != FXP.m_FXSamplers.end() && pSamp->m_dwName[0] == (*it).m_dwName[0])
            //{
            //    assert(*it == *pSamp);
            //    return;
            //}
            //FXP.m_FXSamplers.insert(it, *pSamp);
            //FXP.m_nFlags |= FXP_SAMPLERS_DIRTY;
        }

        private void mfAddFXTexture(CShader pSH, SFXTexture pParam)
        {
            //if (!pSamp)
            //    return;

            //SShaderFXParams & FXP = mfGetFXParams(pSH);

            //FXTexturesIt it = std::lower_bound(FXP.m_FXTextures.begin(), FXP.m_FXTextures.end(), pSamp->m_dwName[0], FXTexturesSortByName());
            //if (it != FXP.m_FXTextures.end() && pSamp->m_dwName[0] == (*it).m_dwName[0])
            //{
            //    assert(*it == *pSamp);
            //    return;
            //}
            //FXP.m_FXTextures.insert(it, *pSamp);
            //FXP.m_nFlags |= FXP_TEXTURES_DIRTY;
        }

        private void mfGeneratePublicFXParams(CShader pSH, CParserBin Parser)
        {
            //SShaderFXParams & FXP = mfGetFXParams(pSH);
            //if (!(FXP.m_nFlags & FXP_PARAMS_DIRTY))
            //    return;
            //FXP.m_nFlags &= ~FXP_PARAMS_DIRTY;

            //uint32 i;
            //// Generate public parameters
            //for (i = 0; i < FXP.m_FXParams.size(); i++)
            //{
            //    SFXParam* pr = &FXP.m_FXParams[i];
            //    uint32 nFlags = pr->GetParamFlags();
            //    if (nFlags & PF_AUTOMERGED)
            //        continue;
            //    if (nFlags & PF_TWEAKABLE_MASK)
            //    {
            //        const char* szName = Parser.GetString(pr->m_dwName[0]);
            //        // Avoid duplicating of public parameters
            //        int32 j;
            //        for (j = 0; j < FXP.m_PublicParams.size(); j++)
            //        {
            //            SShaderParam* p = &FXP.m_PublicParams[j];
            //            if (!stricmp(p->m_Name, szName))
            //                break;
            //        }
            //        if (j == FXP.m_PublicParams.size())
            //        {
            //            SShaderParam sp;
            //            cry_strcpy(sp.m_Name, szName);
            //            EParamType eType;
            //            string szWidget = pr->GetValueForName("UIWidget", eType);
            //            const char* szVal = pr->m_Values.c_str();
            //            if (szWidget == "color")
            //            {
            //                sp.m_Type = eType_FCOLOR;
            //                if (szVal[0] == '{')
            //                    szVal++;
            //                CRY_VERIFY(sscanf(szVal, "%f, %f, %f, %f", &sp.m_Value.m_Color[0], &sp.m_Value.m_Color[1], &sp.m_Value.m_Color[2], &sp.m_Value.m_Color[3]) == 4);
            //            }
            //            else
            //            {
            //                sp.m_Type = eType_FLOAT;
            //                sp.m_Value.m_Float = (float)atof(szVal);
            //            }

            //            bool bAdd = true;
            //            if (!pr->m_Annotations.empty() && gRenDev->IsEditorMode())
            //            {
            //                //EParamType eType;
            //                string sFlt = pr->GetValueForName("Filter", eType);
            //                bool bUseScript = true;
            //                if (!sFlt.empty())
            //                {
            //                    std::vector<string> Filters;
            //                    sParseCSV(sFlt, Filters);
            //                    string strShader = Parser.m_pCurShader->GetName();
            //                    uint32 h;
            //                    for (h = 0; h < Filters.size(); h++)
            //                    {
            //                        if (!strnicmp(Filters[h].c_str(), strShader.c_str(), Filters[h].size()))
            //                            break;
            //                    }
            //                    if (h == Filters.size())
            //                    {
            //                        bUseScript = false;
            //                        bAdd = false;
            //                    }
            //                }

            //                if (bUseScript)
            //                {
            //                    sp.m_Script = pr->m_Annotations.c_str();
            //                }
            //            }

            //            if (bAdd)
            //            {
            //                FXP.m_PublicParams.push_back(sp);
            //            }
            //        }
            //    }
            //}
        }

        public CShaderManBin()
        {
            //m_pCEF = gRenDev? &gRenDev->m_cEF : nullptr;
        }

        public SShaderBin GetBinShader(string szName, bool bInclude, uint nRefCRC32, out bool pbChanged)
        {
//            //static float sfTotalTime = 0.0f;

//            if (pbChanged)
//            {
//                if (gRenDev->IsEditorMode())
//                    *pbChanged = false;
//            }

//            //float fTime0 = iTimer->GetAsyncCurTime();

//            SShaderBin* pSHB = SearchInCache(szName, bInclude);
//            if (pSHB)
//                return pSHB;
//            SShaderBinHeader Header[2];
//            memset(&Header, 0, 2 * sizeof(SShaderBinHeader));
//            stack_string nameFile;
//            string nameBin;
//            FILE* fpSrc = NULL;

//#if !defined(_RELEASE)
//            uint32 nSourceCRC32 = 0;
//#endif

//            const char* szExt = bInclude ? "cfi" : "cfx";
//            // First look for source in Game folder
//            nameFile.Format("%sCryFX/%s.%s", gRenDev->m_cEF.m_ShadersGamePath.c_str(), szName, szExt);
//#if !defined(_RELEASE)
//            {
//                fpSrc = gEnv->pCryPak->FOpen(nameFile.c_str(), "rb");
//                nSourceCRC32 = fpSrc ? gEnv->pCryPak->ComputeCRC(nameFile) : 0;
//            }
//#endif
//            if (!fpSrc)
//            {
//                // Second look in Engine folder
//                nameFile.Format("%s/%sCryFX/%s.%s", "%ENGINE%", gRenDev->m_cEF.m_ShadersPath, szName, szExt);
//#if !defined(_RELEASE)
//                {
//                    fpSrc = gEnv->pCryPak->FOpen(nameFile.c_str(), "rb");
//                    nSourceCRC32 = fpSrc ? gEnv->pCryPak->ComputeCRC(nameFile) : 0;
//                }
//#endif
//            }
//            //char szPath[1024];
//            //getcwd(szPath, 1024);
//            nameBin.Format("%s/%s%s.%s", "%ENGINE%", m_pCEF->m_ShadersCache, szName, bInclude ? "cfib" : "cfxb");
//            FILE* fpDst = NULL;
//            int i = 0, n = 2;

//            // don't load from the shadercache.pak when in editing mode
//            if (CRenderer::CV_r_shadersediting)
//                i = 1;

//            string szDst = m_pCEF->m_szUserPath + (nameBin.c_str() + 9); // skip '%ENGINE%/'
//            byte bValid = 0;
//            float fVersion = (float)FX_CACHE_VER;
//            for (; i < n; i++)
//            {
//                if (fpDst)
//                    gEnv->pCryPak->FClose(fpDst);
//                if (!i)
//                {
//                    if (n == 2)
//                    {
//                        char nameLwr[256];
//                        cry_sprintf(nameLwr, "%s.%s", szName, bInclude ? "cfi" : "cfx");
//                        strlwr(nameLwr);
//                        uint32 dwName = CParserBin::GetCRC32(nameLwr);
//                        FXShaderBinValidCRCItor itor = m_BinValidCRCs.find(dwName);
//                        if (itor != m_BinValidCRCs.end())
//                        {
//                            assert(itor->second == false);
//                            continue;
//                        }
//                    }
//                    fpDst = gEnv->pCryPak->FOpen(nameBin.c_str(), "rb");
//                }
//                else
//                    fpDst = gEnv->pCryPak->FOpen(szDst.c_str(), "rb", ICryPak::FLAGS_NEVER_IN_PAK | ICryPak::FLAGS_PATH_REAL | ICryPak::FOPEN_ONDISK);
//                if (!fpDst)
//                    continue;
//                else
//                {
//                    gEnv->pCryPak->FReadRaw(&Header[i], 1, sizeof(SShaderBinHeader), fpDst);
//                    if (CParserBin::m_bEndians)
//                        SwapEndian(Header[i], eBigEndian);

//#if !defined(_RELEASE)
//                    // check source crc changes
//                    if (nSourceCRC32 && nSourceCRC32 != Header[i].m_nSourceCRC32)
//                    {
//                        bValid |= 1 << i;
//                    }
//                    else
//#endif
//                    {
//                        uint16 MinorVer = (uint16)(((float)fVersion - (float)(int)fVersion) * 10.1f);
//                        uint16 MajorVer = (uint16)fVersion;
//                        if (Header[i].m_VersionLow != MinorVer || Header[i].m_VersionHigh != MajorVer || Header[i].m_Magic != FOURCC_SHADERBIN)
//                            bValid |= 4 << i;
//                        else if (nRefCRC && Header[i].m_CRC32 != nRefCRC)
//                            bValid |= 0x10 << i;
//                    }
//                }
//                if (!(bValid & (0x15 << i)))
//                    break;
//            }
//            if (i == n)
//            {
//#if !defined(_RELEASE) && !defined(CONSOLE_CONST_CVAR_MODE)
//                {
//                    char acTemp[512];
//                    if (bValid & 1)
//                    {
//                        cry_sprintf(acTemp, "WARNING: Bin FXShader '%s' source crc mismatch", nameBin.c_str());
//                    }
//                    if (bValid & 4)
//                    {
//                        cry_sprintf(acTemp, "WARNING: Bin FXShader '%s' version mismatch (Cache: %u.%u, Expected: %.1f)", nameBin.c_str(), Header[0].m_VersionHigh, Header[0].m_VersionLow, fVersion);
//                    }
//                    if (bValid & 0x10)
//                    {
//                        cry_sprintf(acTemp, "WARNING: Bin FXShader '%s' CRC mismatch", nameBin.c_str());
//                    }

//                    if (bValid & 2)
//                    {
//                        cry_sprintf(acTemp, "WARNING: Bin FXShader USER '%s' source crc mismatch", szDst.c_str());
//                    }
//                    if (bValid & 8)
//                    {
//                        cry_sprintf(acTemp, "WARNING: Bin FXShader USER '%s' version mismatch (Cache: %u.%u, Expected: %.1f)", szDst.c_str(), Header[1].m_VersionHigh, Header[1].m_VersionLow, fVersion);
//                    }
//                    if (bValid & 0x20)
//                    {
//                        cry_sprintf(acTemp, "WARNING: Bin FXShader USER '%s' CRC mismatch", szDst.c_str());
//                    }

//                    if (bValid)
//                    {
//                        LogWarningEngineOnly(acTemp);
//                    }

//                    if (fpDst)
//                    {
//                        gEnv->pCryPak->FClose(fpDst);
//                        fpDst = NULL;
//                    }

//                    if (fpSrc)
//                    {
//                        // enable shader compilation again, and show big error message
//                        if (!CRenderer::CV_r_shadersAllowCompilation)
//                        {
//                            if (CRenderer::CV_r_shaderscompileautoactivate)
//                            {
//                                CRenderer::CV_r_shadersAllowCompilation = 1;
//                                CRenderer::CV_r_shadersasyncactivation = 0;

//                                gEnv->pLog->LogError("ERROR LOADING BIN SHADER - REACTIVATING SHADER COMPILATION !");
//                            }
//                            else
//                            {
//                                static bool bShowMessageBox = true;

//                                if (bShowMessageBox)
//                                {
//                                    CryMessageBox(acTemp, "Invalid ShaderCache", eMB_Error);
//                                    bShowMessageBox = false;
//                                    CrySleep(33);
//                                }
//                            }
//                        }

//                        if (CRenderer::CV_r_shadersAllowCompilation)
//                        {
//                            pSHB = SaveBinShader(nSourceCRC32, szName, bInclude, fpSrc);
//                            assert(!pSHB->m_Next);
//                            if (pbChanged)
//                                *pbChanged = true;

//                            // remove the entries in the looupdata, to be sure that level and global caches have also become invalid for these shaders!
//                            gRenDev->m_cEF.m_ResLookupDataMan[static_cast<int>(cacheSource::readonly)].RemoveData(Header[0].m_CRC32);
//                            gRenDev->m_cEF.m_ResLookupDataMan[static_cast<int>(cacheSource::user)].RemoveData(Header[1].m_CRC32);

//                            // has the shader been successfully written to the dest address
//                            fpDst = gEnv->pCryPak->FOpen(szDst.c_str(), "rb", ICryPak::FLAGS_NEVER_IN_PAK | ICryPak::FLAGS_PATH_REAL | ICryPak::FOPEN_ONDISK);
//                            if (fpDst)
//                            {
//                                SAFE_DELETE(pSHB);
//                                i = 1;
//                            }
//                        }
//                    }
//                }
//#endif
//            }
//            if (fpSrc)
//                gEnv->pCryPak->FClose(fpSrc);
//            fpSrc = NULL;

//            if (!CRenderer::CV_r_shadersAllowCompilation)
//            {
//                if (pSHB == 0 && !fpDst)
//                {
//                    //do only perform the necessary stuff
//                    fpDst = gEnv->pCryPak->FOpen(nameBin, "rb");
//                }
//            }
//            if (pSHB == 0 && fpDst)
//            {
//                nameFile.Format("%s.%s", szName, szExt);
//                pSHB = LoadBinShader(fpDst, nameFile.c_str(), i == 0 ? nameBin.c_str() : szDst.c_str(), !bInclude);
//                gEnv->pCryPak->FClose(fpDst);
//                assert(pSHB);
//            }

//            if (pSHB)
//            {
//                if (i == 0)
//                    pSHB->m_bReadOnly = true;
//                else
//                    pSHB->m_bReadOnly = false;

//                AddToCache(pSHB, bInclude);
//                if (!bInclude)
//                {
//                    char nm[128];
//                    nm[0] = '$';
//                    cry_strcpy(&nm[1], sizeof(nm) - 1, szName);
//                    CCryNameTSCRC NM = CParserBin::GetPlatformSpecName(nm);
//                    FXShaderBinPathItor it = m_BinPaths.find(NM);
//                    if (it == m_BinPaths.end())
//                        m_BinPaths.insert(FXShaderBinPath::value_type(NM, i == 0 ? string(nameBin) : szDst));
//                    else
//                        it->second = (i == 0) ? string(nameBin) : szDst;
//                }
//            }
//            else
//            {
//                if (fpDst)
//                    Warning("Error: Failed to get binary shader '%s'", nameFile.c_str());
//                else
//                {
//                    nameFile.Format("%s.%s", szName, szExt);
//                    const char* matName = 0;
//                    if (m_pCEF && m_pCEF->m_pCurInputResources)
//                        matName = m_pCEF->m_pCurInputResources->m_szMaterialName;
//                    LogWarningEngineOnly("Error: Shader \"%s\" doesn't exist (used in material \"%s\")", nameFile.c_str(), matName != 0 ? matName : "$unknown$");
//                }
//            }

//            /*
//               sfTotalTime += iTimer->GetAsyncCurTime() - fTime0;

//               {
//               char acTmp[1024];
//               cry_sprintf(acTmp, "Parsing of bin took: %f \n", sfTotalTime);
//               OutputDebugString(acTmp);
//               }
//             */

//            return pSHB;
        }

        //public bool ParseBinFX(SShaderBin pBin, CShader ef, ulong nMaskGen);
        //public bool ParseBinFX_Dummy(SShaderBin pBin, List<string> ShaderNames, string szName);

        public SShaderFXParams mfGetFXParams(CShader pSH)
        {
            //var s = pSH.GetNameCRC();
            //ShaderFXParamsItor it = m_ShaderFXParams.find(s);
            //if (it != m_ShaderFXParams.end())
            //    return it->second;
            //SShaderFXParams pr;
            //std::pair<ShaderFXParams::iterator, bool> insertLocation =
            //  m_ShaderFXParams.insert(std::pair<CCryNameTSCRC, SShaderFXParams>(s, pr));
            //assert(insertLocation.second);
            //return insertLocation.first->second;
        }

        public void mfRemoveFXParams(CShader pSH)
        {
            //var s = pSH.GetNameCRC();
            //ShaderFXParamsItor it = m_ShaderFXParams.find(s);
            //if (it != m_ShaderFXParams.end())
            //    m_ShaderFXParams.erase(it);
        }

        public int mfSizeFXParams(out uint nCount)
        {
            //nCount = m_ShaderFXParams.Count;
            //int nSize = sizeOfMap(m_ShaderFXParams);
            //return nSize;
        }

        public void mfReleaseFXParams()
        {
            //m_ShaderFXParams.clear();
        }

        public void AddGenMacros(SShaderGen shG, CParserBin Parser, ulong nMaskGen)
        {
            //if (!nMaskGen || !shG)
            //    return;

            //uint32 dwMacro = eT_1;
            //for (uint32 i = 0; i < shG->m_BitMask.Num(); i++)
            //{
            //    if (shG->m_BitMask[i]->m_Mask & nMaskGen)
            //    {
            //        Parser.AddMacro(shG->m_BitMask[i]->m_dwToken, &dwMacro, 1, shG->m_BitMask[i]->m_Mask, Parser.m_Macros[1]);
            //    }
            //}
        }

        public void InvalidateCache(bool bIncludesOnly = false)
        {
            //SShaderBin* pSB, *Next;
            //for (pSB = SShaderBin::s_Root.m_Next; pSB != &SShaderBin::s_Root; pSB = Next)
            //{
            //    Next = pSB->m_Next;
            //    if (bIncludesOnly && !pSB->m_bInclude)
            //        continue;
            //    DeleteFromCache(pSB);
            //}
            //SShaderBin::s_nMaxFXBinCache = MAX_FXBIN_CACHE;
            //m_bBinaryShadersLoaded = false;
            //stl::free_container(s_tempFXParams);

            //g_shaderBucketAllocator.cleanup();
        }

        //public CShaderMan m_pCEF;
        //public FXShaderBinPath m_BinPaths;
        //public FXShaderBinValidCRC m_BinValidCRCs;

        //public bool m_bBinaryShadersLoaded;

        //public ShaderFXParams m_ShaderFXParams;

        //public int Size()
        //{
        //    SShaderBin pSB;
        //    var nSize = 0;
        //    nSize += sizeOfMapStr(m_BinPaths);
        //    nSize += m_BinValidCRCs.size() * sizeof(bool) * sizeof(stl::MapLikeStruct);
        //    for (pSB = SShaderBin::s_Root.m_Prev; pSB != &SShaderBin::s_Root; pSB = pSB->m_Prev)
        //        nSize += pSB->Size();
        //    return nSize;
        //}
        //public void GetMemoryUsage(ICrySizer pSizer)
        //{
        //    SIZER_COMPONENT_NAME(pSizer, "Bin Shaders");
        //    pSizer->AddObject(m_BinPaths);
        //    pSizer->AddObject(m_BinValidCRCs);
        //    SShaderBin pSB;
        //    for (pSB = SShaderBin::s_Root.m_Prev; pSB != &SShaderBin::s_Root; pSB = pSB->m_Prev)
        //        pSB->GetMemoryUsage(pSizer);
        //}
    }
}
