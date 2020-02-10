using CryShader.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ColorF = UnityEngine.Color;
using FXMacro = System.Collections.Generic.Dictionary<string, CryShader.Shaders.SMacroFX>;
using Vec3 = UnityEngine.Vector3;
using Vec4 = UnityEngine.Vector4;

namespace CryShader.Shaders
{
    public struct STokenDesc
    {
        public int id;
        public string token;
    }

    public static class Parser
    {
        static string pCurCommand;
        public static int shGetObject(ref (string s, int i) buf, STokenDesc[] tokens, ref string name, ref string data)
        {
        start:
            if (buf.s[buf.i] == 0)
                return 0;
            SkipCharacters(ref buf, kWhiteSpace);
            SkipComments(ref buf, true);

            if (buf.s[buf.i] == 0)
                return -2;

            string b = buf.s.Substring(buf.i);
            if (b[0] == '#')
            {
                string nam;
                bool bPrepr = false;

                if (b.StartsWith("#if"))
                {
                    bPrepr = true;
                    fxFillPr(ref buf, out nam);
                    fxFillCR(ref buf, out nam);
                    (string s, int i) s = (nam, 0);
                    bool bRes = fxCheckMacroses(ref s, 0);
                    if (b[2] == 'n')
                        bRes = !bRes;
                    if (!bRes)
                    {
                        CParserBin.sfxIFDef.Add(false);
                        fxIgnorePreprBlock(ref buf);
                    }
                    else
                        CParserBin.sfxIFDef.Add(false);
                }
                else if (b.StartsWith("#else"))
                {
                    fxFillPr(ref buf, out nam);
                    bPrepr = true;
                    int nLevel = CParserBin.sfxIFDef.Count - 1;
                    if (nLevel < 0)
                    {
                        Debug.Assert(false);
                        Console.WriteLine("#else without #ifdef");
                        return 0; // false;
                    }
                    if (CParserBin.sfxIFDef[nLevel] == true)
                    {
                        bool bEnded = fxIgnorePreprBlock(ref buf);
                        if (!bEnded)
                        {
                            Debug.Assert(false);
                            Console.WriteLine("#else or #elif after #else");
                            return -1;
                        }
                    }
                }
                else if (b.StartsWith("#elif"))
                {
                    fxFillPr(ref buf, out nam);
                    bPrepr = true;
                    int nLevel = CParserBin.sfxIFDef.Count - 1;
                    if (nLevel < 0)
                    {
                        Debug.Assert(false);
                        Console.WriteLine("#elif without #ifdef");
                        return -1;
                    }
                    if (CParserBin.sfxIFDef[nLevel] == true)
                    {
                        fxIgnorePreprBlock(ref buf);
                    }
                    else
                    {
                        fxFillCR(ref buf, out nam);
                        (string s, int i) = (nam, 0);
                        bool bRes = fxCheckMacroses(ref s, 0);
                        if (!bRes)
                            fxIgnorePreprBlock(ref buf);
                        else
                            CParserBin.sfxIFDef[nLevel] = true;
                    }
                }
                else if (b.StartsWith("#endif"))
                {
                    fxFillPr(ref buf, out nam);
                    bPrepr = true;
                    int nLevel = CParserBin.sfxIFDef.Count - 1;
                    if (nLevel < 0)
                    {
                        Debug.Assert(false);
                        Console.WriteLine("#endif without #ifdef");
                        return -1;
                    }
                    CParserBin.sfxIFDef.RemoveAt(nLevel);
                }
                if (bPrepr)
                    goto start;
            }

            int tokensI = 0;
            STokenDesc token;
            while ((token = tokens[tokensI]).id != 0)
            {
                string bufToken = buf.s.Substring(buf.i, token.token.Length);
                if (string.Equals(token.token, bufToken, StringComparison.OrdinalIgnoreCase))
                {
                    pCurCommand = bufToken;
                    break;
                }
                ++tokensI;
            }
            if ((token = tokens[tokensI]).id == 0)
            {
                int p = buf.s.IndexOf('\n', buf.i);

                string pp;
                if (p != -1)
                {
                    pp = buf.s.Substring(buf.i, p - buf.i);
                    buf.i = p;
                }
                else
                {
                    pp = buf.s.Substring(buf.i);
                }

                Core.iLog.Log("Warning: Found token '{0}' which was not one of the list (Skipping).\n", pp);
                tokensI = 0;
                while ((token = tokens[tokensI]).id != 0)
                {
                    Core.iLog.Log("    %s\n", token.token);
                    tokensI++;
                }
                return 0;
            }
            buf.i += token.token.Length;
            SkipCharacters(ref buf, kWhiteSpace);

            name = GetSubText(ref buf, (char)0x27, (char)0x27);
            SkipCharacters(ref buf, kWhiteSpace);

            if (buf.s[buf.i] == '=')
            {
                ++buf.i;
                data = GetAssignmentText(ref buf);
            }
            else
            {
                data = GetSubText(ref buf, '(', ')');
                if (data != null)
                    data = GetSubText(ref buf, '{', '}');
            }

            return token.id;
        }


        public static string kWhiteSpace = " ,";
        public static FXMacro sStaticMacros = new FXMacro();

        public static bool SkipChar(uint ch)
        {
            bool res = ch <= 0x20;

            res |= (ch - 0x21) < 2;  // !"
            res |= (ch - 0x26) < 10; // &'()*+,-./
            res |= (ch - 0x3A) < 6;  // :;<=>?
            res |= ch == 0x5B;       // [
                                     // cppcheck-suppress badBitmaskCheck
            res |= ch == 0x5D;      // ]
            res |= (ch - 0x7B) < 3; // {|}

            return res;
        }

        public static void fxParserInit() { }

        public static void SkipCharacters(ref (string s, int i) buf, string toSkip)
        {
            char theChar;
            int skip;
            while ((theChar = buf.s[buf.i]) != 0)
            {
                if (theChar >= 0x20)
                {
                    skip = 0;
                    while (toSkip[skip] != 0)
                    {
                        if (theChar == toSkip[skip])
                            break;
                        ++skip;
                    }
                    if (toSkip[skip] == 0)
                        return;
                }
                ++buf.i;
            }
        }

        public static void RemoveCR(ref (string s, int i) buf) => buf = (buf.s.Replace((char)0xd, (char)0x20), buf.i);

        public static void SkipComments(ref (string s, int i) buf, bool bSkipWhiteSpace)
        {
            int n;
            int m;

            while ((n = IsComment(buf)) != 0)
            {
                switch (n)
                {
                    case 2:
                        // skip comment lines.
                        buf.i = buf.s.IndexOf('\n', buf.i);
                        if (buf.s[buf.i] != 0 && bSkipWhiteSpace)
                            SkipCharacters(ref buf, kWhiteSpace);
                        break;

                    case 3:
                        // skip comment blocks.
                        m = 0;
                        do
                        {
                            buf.i = buf.s.IndexOf('*', buf.i);
                            if (buf.s[buf.i] == 0)
                                break;
                            if (buf.s[buf.i - 1] == '/')
                            {
                                buf.i += 1;
                                m++;
                            }
                            else if (buf.s[buf.i + 1] == '/')
                            {
                                buf.i += 2;
                                m--;
                            }
                            else
                                buf.i += 1;
                        }
                        while (m != 0);
                        if (buf.s[buf.i] == 0)
                        {
                            Core.iLog.Log("Warning: Comment lines aren't closed\n");
                            break;
                        }
                        if (bSkipWhiteSpace)
                            SkipCharacters(ref buf, kWhiteSpace);
                        break;
                }
            }
        }

        public static bool fxIsFirstPass((string s, int i) buf)
        {
            string com;
            string tok;
            fxFillCR(ref buf, out com);
            (string s, int i) s = (com, 0);
            while (s.s[s.i] != 0)
            {
                fxFillPr(ref s, out tok);
                if (tok[0] == '%' && tok[1] == '_')
                    return false;
            }
            return true;
        }

        static void fxAddMacro(string Name, string Macro, FXMacro Macros)
        {
            SMacroFX pr;

            if (Name[0] == '%')
            {
                pr.m_nMask = (uint)shGetHex((Macro, 0));
#if _DEBUG
                FXMacroItor it = Macros.find(CONST_TEMP_STRING(Name));
                if (it != Macros.end())
                    assert(0);
#endif
            }
            else
                pr.m_nMask = 0;
            pr.m_szMacro = Macro ?? string.Empty;
            Macros[Name] = pr;
        }

        public static void fxRegisterEnv(string szStr) { } // deprecated

        public static int shFill(ref (string s, int i) buf, out string dst, int nSize = -1)
        {
            int n = 0;
            SkipCharacters(ref buf, kWhiteSpace);
            int pStart = buf.i;
            while (buf.s[buf.i] > 0x20)
            {
                n++;
                ++buf.i;

                if (nSize > 0 && n == nSize)
                {
                    break;
                }
            }

            dst = buf.s.Substring(pStart, n);
            return n;
        }

        public static int fxFill(ref (string s, int i) buf, out string dst, int nSize = -1)
        {
            int n = 0;
            SkipCharacters(ref buf, kWhiteSpace);
            int pStart = buf.i;
            while (buf.s[buf.i] != ';')
            {
                if (buf.s[buf.i] == 0)
                    break;
                n++;
                ++buf.i;

                if (nSize > 0 && n == nSize)
                {
                    dst = buf.s.Substring(pStart, n - 1);
                    return 1;
                }
            }

            dst = buf.s.Substring(pStart, n);
            if (buf.s[buf.i] == ';')
                ++buf.i;

            return n;
        }

        public static (string s, int i) fxFillPr(ref (string s, int i) buf, out string dst)
        {
            int n = 0;
            char ch;
            while ((ch = buf.s[buf.i]) != 0)
            {
                if (!SkipChar(ch))
                    break;
                ++buf.i;
            }
            int pStart = buf.i;
            while ((ch = buf.s[buf.i]) != 0)
            {
                if (SkipChar(ch))
                    break;
                n++;
                ++buf.i;
            }
            dst = buf.s.Substring(pStart, n);
            return (buf.s, pStart);
        }

        public static (string s, int i) fxFillPrC(ref (string s, int i) buf, out string dst)
        {
            int n = 0;
            char ch;
            while ((ch = buf.s[buf.i]) != 0)
            {
                if (!SkipChar(ch))
                    break;
                ++buf.i;
            }
            int pStart = buf.i;
            while ((ch = buf.s[buf.i]) != 0)
            {
                if (ch != ',' && SkipChar(ch))
                    break;
                n++;
                ++buf.i;
            }
            dst = buf.s.Substring(pStart, n);
            return (buf.s, pStart);
        }

        public static (string s, int i) fxFillNumber(ref (string s, int i) buf, out string dst)
        {
            int n = 0;
            char ch;
            while ((ch = buf.s[buf.i]) != 0)
            {
                if (!SkipChar(ch))
                    break;
                ++buf.i;
            }
            int pStart = buf.i;
            while ((ch = buf.s[buf.i]) != 0)
            {
                if (ch != '.' && SkipChar(ch))
                    break;
                n++;
                ++buf.i;
            }
            dst = buf.s.Substring(pStart, n);
            return (buf.s, pStart);
        }

        public static int fxFillCR(ref (string s, int i) buf, out string dst)
        {
            int n = 0;
            SkipCharacters(ref buf, kWhiteSpace);
            int pStart = buf.i;
            while (buf.s[buf.i] != 0xa)
            {
                if (buf.s[buf.i] == 0)
                    break;
                n++;
                ++buf.i;
            }
            dst = buf.s.Substring(pStart, n);
            return n;
        }

        //================================================================================

        public static bool shGetBool((string s, int i) buf)
        {
            if (buf.s[buf.i] == 0)
                return false;

            if (string.Equals(buf.s.Substring(buf.i, 3), "yes", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(buf.s.Substring(buf.i, 4), "true", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(buf.s.Substring(buf.i, 2), "on", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(buf.s.Substring(buf.i, 1), "1", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public static float shGetFloat((string s, int i) buf)
        {
            if (buf.s[buf.i] == 0)
                return 0;
            return G.VERIFY(() => Convert.ToSingle(buf.s.Substring(buf.i)));
        }

        public static void shGetFloat((string s, int i) buf, ref float v1, ref float v2)
        {
            if (buf.s[buf.i] == 0)
                return;
            var parts = buf.s.Substring(buf.i).Split(' ');
            if (parts.Length == 1)
            {
                v1 = v2 = Convert.ToInt32(parts[0]);
            }
            else
            {
                v1 = Convert.ToInt32(parts[0]);
                v2 = Convert.ToInt32(parts[1]);
            }
        }

        public static int shGetInt((string s, int i) buf)
        {
            if (buf.s[buf.i] == 0)
                return 0;
            if (buf.s[buf.i + 0] == '0' && buf.s[buf.i + 1] == 'x')
            {
                return G.VERIFY(() => Convert.ToInt32(buf.s.Substring(buf.i), 16));
            }
            else
            {
                return G.VERIFY(() => Convert.ToInt32(buf.s.Substring(buf.i)));
            }
        }

        public static int shGetHex((string s, int i) buf)
        {
            if (buf.s[buf.i] == 0)
                return 0;
            return G.VERIFY(() => Convert.ToInt32(buf.s.Substring(buf.i), 16));
        }

        public static ulong shGetHex64((string s, int i) buf)
        {
            if (buf.s[buf.i] == 0)
                return 0;
            return G.VERIFY(() => Convert.ToUInt64(buf.s.Substring(buf.i), 16));
        }

        public static void shGetVector((string s, int i) buf, ref Vec3 v)
        {
            if (buf.s[buf.i] == 0)
                return;
            v = G.VERIFY(() =>
            {
                var parts = buf.s.Substring(buf.i).Split(' ');
                return new Vec3(
                    parts.Length >= 0 ? Convert.ToSingle(parts[0]) : 0f,
                    parts.Length >= 1 ? Convert.ToSingle(parts[1]) : 0f,
                    parts.Length >= 2 ? Convert.ToSingle(parts[2]) : 0f);
            });
        }

        public static void shGetVector((string s, int i) buf, ref float[] v)
        {
            if (buf.s[buf.i] == 0)
                return;
            v = G.VERIFY(() =>
            {
                var parts = buf.s.Substring(buf.i).Split(' ');
                return new[] {
                    parts.Length >= 0 ? Convert.ToSingle(parts[0]) : 0f,
                    parts.Length >= 1 ? Convert.ToSingle(parts[1]) : 0f,
                    parts.Length >= 2 ? Convert.ToSingle(parts[2]) : 0f};
            });
        }

        public static void shGetVector4((string s, int i) buf, ref Vec4 v)
        {
            if (buf.s[buf.i] == 0)
                return;
            v = G.VERIFY(() =>
            {
                var parts = buf.s.Substring(buf.i).Split(' ');
                return new Vec4(
                    parts.Length >= 0 ? Convert.ToSingle(parts[0]) : 0f,
                    parts.Length >= 1 ? Convert.ToSingle(parts[1]) : 0f,
                    parts.Length >= 2 ? Convert.ToSingle(parts[2]) : 0f,
                    parts.Length >= 3 ? Convert.ToSingle(parts[3]) : 0f);
            });
        }

        struct SColAsc
        {
            public string nam;
            public ColorF col;

            public SColAsc(string name, ColorF c)
            {
                nam = name;
                col = c;
            }
        }

        static readonly SColAsc[] sCols = new[]
        {
            new SColAsc("Aquamarine", CryColor.Col_Aquamarine),
            new SColAsc("Black", CryColor.Col_Black),
            new SColAsc("Blue", CryColor.Col_Blue),
            new SColAsc("BlueViolet", CryColor.Col_BlueViolet),
            new SColAsc("Brown", CryColor.Col_Brown),
            new SColAsc("CadetBlue", CryColor.Col_CadetBlue),
            new SColAsc("Coral", CryColor.Col_Coral),
            new SColAsc("CornflowerBlue", CryColor.Col_CornflowerBlue),
            new SColAsc("Cyan", CryColor.Col_Cyan),
            new SColAsc("DarkGray", CryColor.Col_DarkGray),
            new SColAsc("DarkGrey", CryColor.Col_DarkGrey),
            new SColAsc("DarkGreen", CryColor.Col_DarkGreen),
            new SColAsc("DarkOliveGreen", CryColor.Col_DarkOliveGreen),
            new SColAsc("DarkOrchid", CryColor.Col_DarkOrchid),
            new SColAsc("DarkSlateBlue", CryColor.Col_DarkSlateBlue),
            new SColAsc("DarkSlateGray", CryColor.Col_DarkSlateGray),
            new SColAsc("DarkSlateGrey", CryColor.Col_DarkSlateGrey),
            new SColAsc("DarkTurquoise", CryColor.Col_DarkTurquoise),
            new SColAsc("DarkWood", CryColor.Col_DarkWood),
            new SColAsc("DeepPink", CryColor.Col_DeepPink),
            new SColAsc("DimGray", CryColor.Col_DimGray),
            new SColAsc("DimGrey", CryColor.Col_DimGrey),
            new SColAsc("FireBrick", CryColor.Col_FireBrick),
            new SColAsc("ForestGreen", CryColor.Col_ForestGreen),
            new SColAsc("Gold", CryColor.Col_Gold),
            new SColAsc("Goldenrod", CryColor.Col_Goldenrod),
            new SColAsc("Gray", CryColor.Col_Gray),
            new SColAsc("Grey", CryColor.Col_Grey),
            new SColAsc("Green", CryColor.Col_Green),
            new SColAsc("GreenYellow", CryColor.Col_GreenYellow),
            new SColAsc("IndianRed", CryColor.Col_IndianRed),
            new SColAsc("Khaki", CryColor.Col_Khaki),
            new SColAsc("LightBlue", CryColor.Col_LightBlue),
            new SColAsc("LightGray", CryColor.Col_LightGray),
            new SColAsc("LightGrey", CryColor.Col_LightGrey),
            new SColAsc("LightSteelBlue", CryColor.Col_LightSteelBlue),
            new SColAsc("LightWood", CryColor.Col_LightWood),
            new SColAsc("Lime", CryColor.Col_Lime),
            new SColAsc("LimeGreen", CryColor.Col_LimeGreen),
            new SColAsc("Magenta", CryColor.Col_Magenta),
            new SColAsc("Maroon", CryColor.Col_Maroon),
            new SColAsc("MedianWood", CryColor.Col_MedianWood),
            new SColAsc("MediumAquamarine", CryColor.Col_MediumAquamarine),
            new SColAsc("MediumBlue", CryColor.Col_MediumBlue),
            new SColAsc("MediumForestGreen", CryColor.Col_MediumForestGreen),
            new SColAsc("MediumGoldenrod", CryColor.Col_MediumGoldenrod),
            new SColAsc("MediumOrchid", CryColor.Col_MediumOrchid),
            new SColAsc("MediumSeaGreen", CryColor.Col_MediumSeaGreen),
            new SColAsc("MediumSlateBlue", CryColor.Col_MediumSlateBlue),
            new SColAsc("MediumSpringGreen", CryColor.Col_MediumSpringGreen),
            new SColAsc("MediumTurquoise", CryColor.Col_MediumTurquoise),
            new SColAsc("MediumVioletRed", CryColor.Col_MediumVioletRed),
            new SColAsc("MidnightBlue", CryColor.Col_MidnightBlue),
            new SColAsc("Navy", CryColor.Col_Navy),
            new SColAsc("NavyBlue", CryColor.Col_NavyBlue),
            new SColAsc("Orange", CryColor.Col_Orange),
            new SColAsc("OrangeRed", CryColor.Col_OrangeRed),
            new SColAsc("Orchid", CryColor.Col_Orchid),
            new SColAsc("PaleGreen", CryColor.Col_PaleGreen),
            new SColAsc("Pink", CryColor.Col_Pink),
            new SColAsc("Plum", CryColor.Col_Plum),
            new SColAsc("Red", CryColor.Col_Red),
            new SColAsc("Salmon", CryColor.Col_Salmon),
            new SColAsc("SeaGreen", CryColor.Col_SeaGreen),
            new SColAsc("Sienna", CryColor.Col_Sienna),
            new SColAsc("SkyBlue", CryColor.Col_SkyBlue),
            new SColAsc("SlateBlue", CryColor.Col_SlateBlue),
            new SColAsc("SpringGreen", CryColor.Col_SpringGreen),
            new SColAsc("SteelBlue", CryColor.Col_SteelBlue),
            new SColAsc("Tan", CryColor.Col_Tan),
            new SColAsc("Thistle", CryColor.Col_Thistle),
            new SColAsc("Turquoise", CryColor.Col_Turquoise),
            new SColAsc("Violet", CryColor.Col_Violet),
            new SColAsc("VioletRed", CryColor.Col_VioletRed),
            new SColAsc("Wheat", CryColor.Col_Wheat),
            new SColAsc("White", CryColor.Col_White),
            new SColAsc("Yellow", CryColor.Col_Yellow),
            new SColAsc("YellowGreen", CryColor.Col_YellowGreen),

            new SColAsc(null, new ColorF(1.0f, 1.0f, 1.0f))
        };

        public static void shGetColor((string s, int i) buf, ref ColorF v)
        {
            string name;
            int n;
            if (buf.s[buf.i] == 0)
            {
                v = CryColor.Col_White;
                return;
            }
            if (buf.s[buf.i] == '{')
                buf.i++;
            if (char.IsLetter(buf.s[buf.i]))
            {
                n = 0;
                float scal = 1;
                name = buf.s.Substring(buf.i, 64);
                string nm;
                int pStart = buf.i;
                if (buf.s.IndexOf('*', buf.i) != 0)
                {
                    while (buf.s[buf.i + n] != '*')
                    {
                        if (buf.s[buf.i + n] == 0x20)
                            break;
                        n++;
                    }
                    nm = buf.s.Substring(pStart, n);
                    if (buf.s[buf.i + n] == 0x20)
                    {
                        while (buf.s[buf.i + n] != '*')
                            n++;
                    }
                    n++;
                    while (buf.s[buf.i + n] == 0x20)
                        n++;
                    scal = shGetFloat((buf.s, buf.i + n));
                    name = nm;
                }
                n = 0;
                while (sCols[n].nam != null)
                {
                    if (sCols[n].nam == name)
                    {
                        v = sCols[n].col;
                        if (scal != 1)
                            v.ScaleCol(scal);
                        return;
                    }
                    n++;
                }
            }
            n = 0;
            while (true)
            {
                if (n == 4)
                    break;
                string par;
                fxFillNumber(ref buf, out par);
                if (par[0] == 0)
                    break;
                v[n++] = Convert.ToSingle(par);
            }
        }

        public static void shGetColor((string s, int i) buf, float[] v)
        {
            string name;
            int n;
            if (buf.s[buf.i] == 0)
            {
                v[0] = 1.0f;
                v[1] = 1.0f;
                v[2] = 1.0f;
                v[3] = 1.0f;
                return;
            }
            if (char.IsLetter(buf.s[buf.i]))
            {
                n = 0;
                float scal = 1;
                name = buf.s.Substring(buf.i, 64);
                string nm;
                if (buf.s.IndexOf('*', buf.i) != 0)
                {
                    while (buf.s[buf.i + n] != '*')
                    {
                        if (buf.s[buf.i + n] == 0x20)
                            break;
                        n++;
                    }
                    nm = buf.s.Substring(buf.i, n);
                    if (buf.s[buf.i + n] == 0x20)
                    {
                        while (buf.s[buf.i + n] != '*')
                            n++;
                    }
                    n++;
                    while (buf.s[buf.i + n] == 0x20)
                        n++;
                    scal = shGetFloat((buf.s, buf.i + n));
                    name = nm;
                }
                n = 0;
                while (sCols[n].nam != null)
                {
                    if (sCols[n].nam == name)
                    {
                        v[0] = sCols[n].col[0];
                        v[1] = sCols[n].col[1];
                        v[2] = sCols[n].col[2];
                        v[3] = sCols[n].col[3];
                        if (scal != 1)
                        {
                            v[0] *= scal;
                            v[1] *= scal;
                            v[2] *= scal;
                        }
                        return;
                    }
                    n++;
                }
            }
            var parts = buf.s.Substring(buf.i).Split(' ');
            v[0] = parts.Length >= 0 ? Convert.ToSingle(parts[0]) : 1.0f;
            v[1] = parts.Length >= 1 ? Convert.ToSingle(parts[1]) : 1.0f;
            v[2] = parts.Length >= 2 ? Convert.ToSingle(parts[2]) : 1.0f;
            v[3] = parts.Length >= 3 ? Convert.ToSingle(parts[3]) : 1.0f;
        }

        //public static int shGetVar(const char** buf, char** vr, char** val)

        //=========================================================================================

        static string GetAssignmentText(ref (string s, int i) buf)
        {
            SkipCharacters(ref buf, kWhiteSpace);
            int pStart = buf.i;
            string result;

            char theChar;
            while ((theChar = buf.s[buf.i]) != 0)
            {
                if (theChar == '[')
                {
                    while ((theChar = buf.s[buf.i]) != ']')
                    {
                        if (theChar == 0 || theChar == ';')
                            break;
                        ++buf.i;
                    }
                    continue;
                }
                if (theChar <= 0x20 || theChar == ';')
                    break;
                ++buf.i;
            }

            result = buf.s.Substring(pStart, buf.i - pStart);
            if (theChar != 0)
                ++buf.i;
            return result;
        }

        static string GetSubText(ref (string s, int i) buf, char open, char close)
        {
            if (buf.s[buf.i] == 0 || buf.s[buf.i] != open)
                return null;
            ++buf.i;
            int pStart = buf.i;
            string result = null;

            char theChar;
            long skip = 1;
            if (open == close)
                open = (char)0;
            while ((theChar = buf.s[buf.i]) != 0)
            {
                if (theChar == open)
                    ++skip;
                if (theChar == close)
                {
                    if (--skip == 0)
                    {
                        result = buf.s.Substring(pStart, buf.i - pStart);
                        ++buf.i;
                        break;
                    }
                }
                ++buf.i;
            }
            return result;
        }

        static int IsComment((string s, int i) buf)
        {
            if (buf.s[buf.i] == 0)
                return 0;

            if (buf.s[buf.i + 0] == '/' && buf.s[buf.i + 1] == '/')
                return 2;

            if (buf.s[buf.i + 0] == '/' && buf.s[buf.i + 1] == '*')
                return 3;

            return 0;
        }

        static void fxSkipTillCR(ref (string s, int i) buf)
        {
            char ch;
            while ((ch = buf.s[buf.i]) != 0)
            {
                if (ch == 0xa)
                    break;
                ++buf.i;
            }
        }

        static bool fxCheckMacroses(ref (string s, int i) str, int nPass)
        {
            string tmpBuf;
            bool[] bRes = new bool[64];
            bool[] bOr = new bool[64];
            int nLevel = 0;
            int i;
            while (true)
            {
                SkipCharacters(ref str, kWhiteSpace);
                if (str.s[str.i] == '(')
                {
                    ++str.i;
                    int n = 0;
                    int nD = 0;
                    int pStart = str.i;
                    while (true)
                    {
                        if (str.s[str.i] == '(')
                            n++;
                        else if (str.s[str.i] == ')')
                        {
                            if (n == 0)
                            {
                                tmpBuf = str.s.Substring(pStart, nD);
                                ++str.i;
                                break;
                            }
                            n--;
                        }
                        else if (str.s[str.i] == 0)
                            return false;
                        nD++;
                        ++str.i;
                    }
                    (string s, int i) s = (tmpBuf, 0);
                    bRes[nLevel] = fxCheckMacroses(ref s, nPass);
                    nLevel++;
                    bOr[nLevel] = true;
                }
                else
                {
                    int pStart = str.i;
                    int n = 0;
                    while (true)
                    {
                        if (str.s[str.i] == '|' || str.s[str.i] == '&' || str.s[str.i] == 0)
                            break;
                        if (str.s[str.i] <= 0x20)
                            break;
                        n++;
                        ++str.i;
                    }
                    tmpBuf = str.s.Substring(pStart, n);
                    if (tmpBuf[0] != 0)
                    {
                        (string s, int i) s = (tmpBuf, 0);
                        bool bNeg = false;
                        if (s.s[0] == '!')
                        {
                            bNeg = true;
                            s.i++;
                        }
                        SMacroBinFX? pFound;
                        if (char.IsLetter(s.s[s.i]))
                        {
                            if ((s.s[s.i] == '0' && s.s[s.i + 1] == 'x') || s.s[s.i] != 0)
                                pFound = SMacroBinFX.Empty;
                            else
                                pFound = null;
                        }
                        else
                        {
                            bool bKey = false;
                            byte[] tmpBuf2 = new byte[1024];
                            uint nTok = CParserBin.NextToken(s, tmpBuf2, out bKey);
                            if (nTok == (uint)EToken.eT_unknown)
                                nTok = CParserBin.GetCRC32(tmpBuf);
                            pFound = CParserBin.FindMacro(nTok, CParserBin.GetStaticMacroses());
                        }
                        bRes[nLevel] = pFound != null ? true : false;
                        if (bNeg)
                            bRes[nLevel] = !bRes[nLevel];
                        nLevel++;
                        bOr[nLevel] = true;
                    }
                    else
                        Debug.Assert(false);
                }
                SkipCharacters(ref str, kWhiteSpace);
                if (str.s[str.i] == 0)
                    break;
                if (str.s[str.i] == '|' && str.s[str.i + 1] == '|')
                {
                    bOr[nLevel] = true;
                    str.i += 2;
                }
                else if (str.s[str.i] == '&' && str.s[str.i + 1] == '&')
                {
                    bOr[nLevel] = false;
                    str.i += 2;
                }
                else
                    Debug.Assert(false);
            }
            bool Res = false;
            for (i = 0; i < nLevel; i++)
            {
                if (i == 0)
                    Res = bRes[i];
                else
                {
                    Debug.Assert(bOr[i] != true);
                    if (bOr[i])
                        Res = Res | bRes[i];
                    else
                        Res = Res & bRes[i];
                }
            }
            return Res;
        }

        static bool fxIgnorePreprBlock(ref (string s, int i) buf)
        {
            int nLevel = 0;
            bool bEnded = false;
            SkipCharacters(ref buf, kWhiteSpace);
            SkipComments(ref buf, true);

            while (buf.s[buf.i] != 0)
            {
                char ch;
                while ((ch = buf.s[buf.i]) != 0 && SkipChar((uint)ch))
                {
                    while ((ch = buf.s[buf.i]) != 0)
                    {
                        if (ch == '/' && IsComment(buf) != 0)
                            break;
                        if (!SkipChar(ch))
                            break;
                        ++buf.i;
                    }
                    SkipComments(ref buf, true);
                }
                (string s, int i) posS = buf;
                (string s, int i) st = posS;
                if (posS.s[posS.i] == '#')
                {
                    posS.i++;
                    if (SkipChar(posS.s[posS.i]))
                    {
                        while ((ch = posS.s[posS.i]) != 0)
                        {
                            if (!SkipChar(ch))
                                break;
                            posS.i++;
                        }
                    }
                    if (posS.s[posS.i] == 'i' && posS.s[posS.i + 1] == 'f')
                    {
                        nLevel++;
                        buf.i = posS.i + 2;
                        continue;
                    }
                    if (posS.s.Substring(posS.i, 5) == "endif")
                    {
                        if (nLevel == 0)
                        {
                            buf.i = st.i;
                            bEnded = true;
                            break;
                        }
                        nLevel--;
                        buf.i = posS.i + 4;
                    }
                    else if (posS.s.Substring(posS.i, 4) == "else" || posS.s.Substring(posS.i, 4) == "elif")
                    {
                        if (nLevel == 0)
                        {
                            buf.i = st.i;
                            break;
                        }
                        buf.i = posS.i + 4;
                    }
                }
                while ((ch = buf.s[buf.i]) != 0)
                {
                    if (ch == '/' && IsComment(buf) != 0)
                        break;
                    if (SkipChar(ch))
                        break;
                    ++buf.i;
                }
            }
            if (buf.s[buf.i] == 0)
            {
                Debug.Assert(false);
                Console.WriteLine("Couldn't find #endif directive for associated #ifdef");
                return false;
            }

            return bEnded;
        }
    }
}
