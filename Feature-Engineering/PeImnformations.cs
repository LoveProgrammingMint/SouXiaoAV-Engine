using PeNet;
using PeNet.Header.Pe;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Feature_Engineering
{
    public class PEInformations
    {
        public bool Label { get; set; }
        public float Probability { get; set; }
        public float FileEntropy { get; set; }
        public float TextSection { get; set; }
        public float TextSizeRatio { get; set; }
        public float DataSection { get; set; }
        public float DataSizeRatio { get; set; }
        public float RsrcSection { get; set; }
        public float RsrcSizeRatio { get; set; }
        public float IsExe { get; set; }
        public float IsDll { get; set; }
        public float IsDriver { get; set; }
        public float Is64Bit { get; set; }
        public float IsDebug { get; set; }
        public float SpecialBuildLength { get; set; }
        public float PrivateBuildLength { get; set; }
        public float TrustSigned { get; set; }
        public float ExceptionCount { get; set; }
        public float Machine { get; set; }
        public float NumberOfSections { get; set; }
        public float TimeDateStamp { get; set; }
        public float PointerToSymbolTable { get; set; }
        public float NumberOfSymbols { get; set; }
        public float SizeOfOptionalHeader { get; set; }
        public float Characteristics { get; set; }
        public float Magic { get; set; }
        public float MajorLinkerVersion { get; set; }
        public float MinorLinkerVersion { get; set; }
        public float SizeOfCode { get; set; }
        public float SizeOfInitializedData { get; set; }
        public float SizeOfUninitializedData { get; set; }
        public float AddressOfEntryPoint { get; set; }
        public float BaseOfCode { get; set; }
        public float ImageBase { get; set; }
        public float SectionAlignment { get; set; }
        public float FileAlignment { get; set; }
        public float MajorOperatingSystemVersion { get; set; }
        public float MinorOperatingSystemVersion { get; set; }
        public float MajorImageVersion { get; set; }
        public float MinorImageVersion { get; set; }
        public float MajorSubsystemVersion { get; set; }
        public float MinorSubsystemVersion { get; set; }
        public float SizeOfImage { get; set; }
        public float SizeOfHeaders { get; set; }
        public float CheckSum { get; set; }
        public float Subsystem { get; set; }
        public float DllCharacteristics { get; set; }
        public float SizeOfStackReserve { get; set; }
        public float SizeOfStackCommit { get; set; }
        public float SizeOfHeapReserve { get; set; }
        public float SizeOfHeapCommit { get; set; }
        public float LoaderFlags { get; set; }
        public float NumberOfRvaAndSizes { get; set; }
        public float ApiCount { get; set; }
        public float ExportCount { get; set; }
        public float FileDescriptionLength { get; set; }
        public float FileVersionLength { get; set; }
        public float ProductNameLength { get; set; }
        public float ProductVersionLength { get; set; }
        public float CompanyNameLength { get; set; }
        public float LegalCopyrightLength { get; set; }
        public float CommentsLength { get; set; }
        public float InternalNameLength { get; set; }
        public float LegalTrademarksLength { get; set; }
        public float ExecutableSections { get; set; }
        public float WritableSections { get; set; }
        public float ReadableSections { get; set; }
        public float SectionCount { get; set; }
        public float SectionException { get; set; }
        public float DebugCount { get; set; }
        public float IsPatched { get; set; }
        public float IsPrivateBuild { get; set; }
        public float IsPreRelease { get; set; }
        public float IsSpecialBuild { get; set; }
        public float IconCount { get; set; }
        public float IsAdmin { get; set; }
        public float IsInstall { get; set; }
        public float HasTlsCallbacks { get; set; }
        public float HasInvalidTimestamp { get; set; }
        public float HasRelocationDirectory { get; set; }

        public Dictionary<string, double> ToDict()
        {
            var dict = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var p in GetType().GetProperties())
            {
                var val = p.GetValue(this);
                double v = val switch
                {
                    bool b => b ? 1 : 0,
                    float f => f,
                    double d => d,
                    int i => i,
                    _ => 0
                };
                dict[p.Name] = v;
            }
            return dict;
        }
    }

    public static class PEFeatureExtractor
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int memcmp(byte* b1, byte* b2, int count);

        public static Dictionary<string, double> ExtractFeatures(string filePath)
        {
            var data = new PEInformations();
            try
            {
                if (!PeFile.IsPeFile(filePath)) return [];
                var fileBytes = File.ReadAllBytes(filePath);
                var pe = new PeFile(fileBytes);
                var fileInfo = FileVersionInfo.GetVersionInfo(filePath);

                data.FileEntropy = (float)CalculateEntropy(fileBytes);

                data.FileDescriptionLength = string.IsNullOrEmpty(fileInfo.FileDescription) ? 0 : fileInfo.FileDescription.Length;
                data.FileVersionLength = string.IsNullOrEmpty(fileInfo.FileVersion) ? 0 : fileInfo.FileVersion.Length;
                data.ProductNameLength = string.IsNullOrEmpty(fileInfo.ProductName) ? 0 : fileInfo.ProductName.Length;
                data.ProductVersionLength = string.IsNullOrEmpty(fileInfo.ProductVersion) ? 0 : fileInfo.ProductVersion.Length;
                data.CompanyNameLength = string.IsNullOrEmpty(fileInfo.CompanyName) ? 0 : fileInfo.CompanyName.Length;
                data.LegalCopyrightLength = string.IsNullOrEmpty(fileInfo.LegalCopyright) ? 0 : fileInfo.LegalCopyright.Length;
                data.SpecialBuildLength = string.IsNullOrEmpty(fileInfo.SpecialBuild) ? 0 : fileInfo.SpecialBuild.Length;
                data.PrivateBuildLength = string.IsNullOrEmpty(fileInfo.PrivateBuild) ? 0 : fileInfo.PrivateBuild.Length;
                data.CommentsLength = string.IsNullOrEmpty(fileInfo.Comments) ? 0 : fileInfo.Comments.Length;
                data.InternalNameLength = string.IsNullOrEmpty(fileInfo.InternalName) ? 0 : fileInfo.InternalName.Length;
                data.LegalTrademarksLength = string.IsNullOrEmpty(fileInfo.LegalTrademarks) ? 0 : fileInfo.LegalTrademarks.Length;

                data.IsDebug = fileInfo.IsDebug ? 1 : 0;
                data.IsPatched = fileInfo.IsPatched ? 1 : 0;
                data.IsPrivateBuild = fileInfo.IsPrivateBuild ? 1 : 0;
                data.IsPreRelease = fileInfo.IsPreRelease ? 1 : 0;
                data.IsSpecialBuild = fileInfo.IsSpecialBuild ? 1 : 0;

                if (pe.ImageDebugDirectory != null) data.DebugCount = pe.ImageDebugDirectory.Length;
                if (pe.Resources?.Icons != null) data.IconCount = pe.Resources.Icons.Length;

                if (pe.ImageTlsDirectory?.AddressOfCallBacks > 0) data.HasTlsCallbacks = 1;
                data.HasInvalidTimestamp = pe.ImageNtHeaders?.FileHeader.TimeDateStamp > DateTime.UtcNow.ToFileTime() ? 1 : 0;
                data.HasRelocationDirectory = pe.ImageNtHeaders?.OptionalHeader.DataDirectory[5].Size > 0 ? 1 : 0;

                if (pe.ImageSectionHeaders != null)
                {
                    foreach (var s in pe.ImageSectionHeaders)
                    {
                        if ((s.Characteristics & ScnCharacteristicsType.MemExecute) != 0) data.ExecutableSections++;
                        if ((s.Characteristics & ScnCharacteristicsType.MemWrite) != 0) data.WritableSections++;
                        if ((s.Characteristics & ScnCharacteristicsType.MemRead) != 0) data.ReadableSections++;
                        if (s.SizeOfRawData + s.PointerToRawData > fileBytes.Length) data.SectionException = 1;

                        if (s.Name.Equals(".text"))
                        {
                            var span = fileBytes.AsSpan((int)s.PointerToRawData, (int)s.SizeOfRawData);
                            data.TextSection = (float)CalculateEntropy(span.ToArray());
                            data.TextSizeRatio = s.SizeOfRawData / (float)pe.FileSize;
                        }
                        else if (s.Name.Equals(".data"))
                        {
                            var span = fileBytes.AsSpan((int)s.PointerToRawData, (int)s.SizeOfRawData);
                            data.DataSection = (float)CalculateEntropy(span.ToArray());
                            data.DataSizeRatio = s.SizeOfRawData / (float)pe.FileSize;
                        }
                        else if (s.Name.Equals(".rsrc"))
                        {
                            var span = fileBytes.AsSpan((int)s.PointerToRawData, (int)s.SizeOfRawData);
                            data.RsrcSection = (float)CalculateEntropy(span.ToArray());
                            data.RsrcSizeRatio = s.SizeOfRawData / (float)pe.FileSize;
                            if (data.IsAdmin != 1) data.IsAdmin = HasTargetStringInBytes(span.ToArray(), "requireAdministrator", 1024) ? 1 : 0;
                            if (data.IsInstall != 1) data.IsInstall = HasTargetStringInBytes(span.ToArray(), "Nullsoft.NSIS.exehead", 2048) ||
                                                                   HasTargetStringInBytes(span.ToArray(), "JR.Inno.Setup", 2048) ||
                                                                   HasTargetStringInBytes(span.ToArray(), "7-Zip.7zipInstall", 2048) ||
                                                                   HasTargetStringInBytes(span.ToArray(), "WinRAR SFX", 2048) ? 1 : 0;
                        }
                    }
                }

                if (pe.ImportedFunctions != null) data.ApiCount = pe.ImportedFunctions.Length;
                if (pe.ExportedFunctions != null) data.ExportCount = pe.ExportedFunctions.Length;

                data.IsExe = pe.IsExe ? 1 : 0;
                data.IsDll = pe.IsDll ? 1 : 0;
                data.IsDriver = pe.IsDriver ? 1 : 0;
                data.Is64Bit = pe.Is64Bit ? 1 : 0;

                data.TrustSigned = WinTrust.VerifyFileSignature(filePath) ? 1 : 0;

                if (pe.ExceptionDirectory != null) data.ExceptionCount = pe.ExceptionDirectory.Length;
                if (pe.ImageSectionHeaders != null) data.SectionCount = pe.ImageSectionHeaders.Length;

                var opt = pe.ImageNtHeaders?.OptionalHeader;
                var fh = pe.ImageNtHeaders?.FileHeader;
                if (opt != null && fh != null)
                {
                    data.Machine = (float)fh.Machine;
                    data.NumberOfSections = fh.NumberOfSections;
                    data.TimeDateStamp = fh.TimeDateStamp;
                    data.PointerToSymbolTable = fh.PointerToSymbolTable;
                    data.NumberOfSymbols = fh.NumberOfSymbols;
                    data.SizeOfOptionalHeader = fh.SizeOfOptionalHeader;
                    data.Characteristics = (float)fh.Characteristics;
                    data.Magic = (float)opt.Magic;
                    data.MajorLinkerVersion = opt.MajorLinkerVersion;
                    data.MinorLinkerVersion = opt.MinorLinkerVersion;
                    data.SizeOfCode = opt.SizeOfCode;
                    data.SizeOfInitializedData = opt.SizeOfInitializedData;
                    data.SizeOfUninitializedData = opt.SizeOfUninitializedData;
                    data.AddressOfEntryPoint = opt.AddressOfEntryPoint;
                    data.BaseOfCode = opt.BaseOfCode;
                    data.ImageBase = opt.ImageBase;
                    data.SectionAlignment = opt.SectionAlignment;
                    data.FileAlignment = opt.FileAlignment;
                    data.MajorOperatingSystemVersion = opt.MajorOperatingSystemVersion;
                    data.MinorOperatingSystemVersion = opt.MinorOperatingSystemVersion;
                    data.MajorImageVersion = opt.MajorImageVersion;
                    data.MinorImageVersion = opt.MinorImageVersion;
                    data.MajorSubsystemVersion = opt.MajorSubsystemVersion;
                    data.MinorSubsystemVersion = opt.MinorSubsystemVersion;
                    data.SizeOfImage = opt.SizeOfImage;
                    data.SizeOfHeaders = opt.SizeOfHeaders;
                    data.CheckSum = opt.CheckSum;
                    data.Subsystem = (float)opt.Subsystem;
                    data.DllCharacteristics = (float)opt.DllCharacteristics;
                    data.SizeOfStackReserve = opt.SizeOfStackReserve;
                    data.SizeOfStackCommit = opt.SizeOfStackCommit;
                    data.SizeOfHeapReserve = opt.SizeOfHeapReserve;
                    data.SizeOfHeapCommit = opt.SizeOfHeapCommit;
                    data.LoaderFlags = opt.LoaderFlags;
                    data.NumberOfRvaAndSizes = opt.NumberOfRvaAndSizes;
                }
            }
            catch {  }

            return data.ToDict();
        }

        public static unsafe double CalculateEntropy(byte[] data)
        {
            const int Size = 256;
            int* freq = stackalloc int[Size];
            for (int i = 0; i < Size; i++) freq[i] = 0;
            fixed (byte* p = data)
            {
                byte* ptr = p, end = p + data.Length;
                while (end - ptr >= 4)
                {
                    freq[ptr[0]]++; freq[ptr[1]]++;
                    freq[ptr[2]]++; freq[ptr[3]]++;
                    ptr += 4;
                }
                while (ptr < end) freq[*ptr++]++;
            }
            double len = data.Length;
            if (len == 0) return 0;
            double inv = 1.0 / len, invLog2 = 1.0 / Math.Log(2);
            double e = 0;
            for (int i = 0; i < Size; i++)
            {
                int f = freq[i];
                if (f > 0)
                {
                    double pv = f * inv;
                    e -= pv * (Math.Log(pv) * invLog2);
                }
            }
            return e;
        }

        public static unsafe bool HasTargetStringInBytes(byte[] src, string txt, int searchLen)
        {
            byte[] pat = Encoding.ASCII.GetBytes(txt);
            int pl = pat.Length;
            if (pl == 0) return true;
            if (src.Length < pl) return false;
            int start = Math.Max(0, src.Length - searchLen);
            int end = src.Length - pl;
            if (start > end) return false;
            fixed (byte* pSrc = src, pPat = pat)
            {
                byte* pStart = pSrc + start, pEnd = pSrc + end;
                for (byte* cur = pEnd; cur >= pStart; cur--)
                    if (memcmp(cur, pPat, pl) == 0) return true;
            }
            return false;
        }
    }
}