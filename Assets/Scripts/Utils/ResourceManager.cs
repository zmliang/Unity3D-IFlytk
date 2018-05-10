using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine;


namespace JinkeGroup.Util
{
    public static class ResourceManager
    {

        private static AsyncOperation UnloadingResources;

        private const string Tag = "ResourceManager";

        public class DecompressInfo
        {
            public float Progress = 0.0f;
            public bool FailedDecompressing;
            public bool Done = false;
        }

#if DOWNLOADABLE_ASSET_BUNDLES

        public class AssetBundlePackage {
            public Dictionary<string,ZipArchiveEntry> ZipArchiveEntries = new Dictionary<string, ZipArchiveEntry>();
            public List<string> ZipArchiveKeys = new List<string>();
            public List<string> ExtractableZipArchiveKeys = new List<string>();
            public ZipArchive ZipArchive;
            public string BundleDiskPath;
            public string BundleFileName;
            public string ResourceDir;

            public string AssetBundleDiskURI {
                get {
                    return "file://" + BundleDiskPath;
                }
            }
        }

        private static AssetBundlePackage Package = new AssetBundlePackage();

        private static readonly Dictionary<string,AssetBundle> LoadedAssetBundles = new Dictionary<string,AssetBundle>();

        private static List<string> RemoveKeys = new List<string>();

        private const string DefaultResourceName = "";

        private static Regex InAssetBundle;

        private static Regex Unpacked;

        private static HashSet<string> FailedDecompressedBundles = new HashSet<string>();

#endif

        public static void UnloadUnusedResources()
        {
#if DOWNLOADABLE_ASSET_BUNDLES
            RemoveKeys.Clear();
            foreach (KeyValuePair<string, AssetBundle > pair in LoadedAssetBundles) {
                pair.Value.Unload(false);
                if (pair.Value == null) {
                    RemoveKeys.Add(pair.Key);
                }
            }

            for (int i = 0; i < RemoveKeys.Count; i++) {
                JinkeGroup.Util.Logger.DebugT(Tag, "UnloadingResources: {0}", RemoveKeys[i]);
                LoadedAssetBundles.Remove(RemoveKeys[i]);
            }
#endif
            //Only allow 1 Unload opertaion at once. Currently (Unity 4.5) if we spam more Resources.UnloadUnusedAssets() level loading basically stops (Noticed by big delays on wardrobe/edit room exit in the Editor and on Android)
            if (UnloadingResources == null || UnloadingResources.isDone)
            {
                JinkeGroup.Util.Logger.DebugT(Tag, "UnloadingResources");
                UnloadingResources = Resources.UnloadUnusedAssets();
            }

        }


        public static void Setup(Regex resourcesInAssetBundle, Regex unpackedResources)
        {
#if DOWNLOADABLE_ASSET_BUNDLES
            InAssetBundle = resourcesInAssetBundle;
            Unpacked = unpackedResources;

            Package.BundleFileName = "resources.assets";
            Package.BundleDiskPath = Path.Combine(Path.Combine(Application.persistentDataPath, "UnityAssets"), Package.BundleFileName);
            Package.ResourceDir = Path.Combine(Application.persistentDataPath, "UnityAssets/Resources");
            AssetBundlePackage package = Package;

            SetupArchiveForReading();
            JinkeGroup.Util.Logger.DebugT(Tag, "Resource bundle loaded: {0}", package.BundleFileName);
#endif
        }

#if DOWNLOADABLE_ASSET_BUNDLES

        private static void RemoveCachedAssetBundle(string diskPath) {
            JinkeGroup.Util.Logger.Debug("RemoveCachedAssetBundle");
            try {
                System.IO.File.Delete(diskPath);//Allow it to crash if it can't delete the file
            } catch (DirectoryNotFoundException e) {
                JinkeGroup.Util.Logger.Debug("Directory Not Found: {0}", e.ToString());
            }
        }
#endif
        public static Object Load(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
#if DOWNLOADABLE_ASSET_BUNDLES
            Object resource = null;
            if (InAssetBundle.IsMatch(id))
                resource = Resources.Load(id);
            else {
                AssetBundle bundle = LoadAssetBundle(id);
                if (bundle != null)
                    resource = bundle.mainAsset;
            }

            if (resource == null) {
                //                throw new System.Exception("Resource load failed on: " + id);
            }
            return resource;
#else
            Object resource = Resources.Load(id);
            if (resource == null)
            {
                //                throw new System.Exception("Resource load failed on: " + id);
            }
            return resource;
#endif
        }


        public static Object Load(string id, System.Type type)
        {
            if (string.IsNullOrEmpty(id))
                return null;
#if DOWNLOADABLE_ASSET_BUNDLES
            Object resource = null;
            if (InAssetBundle.IsMatch(id))
                resource = Resources.Load(id, type);
            else {
                AssetBundle bundle = LoadAssetBundle(id);
                if (bundle != null)
                    resource = bundle.mainAsset;
            }
            if (resource == null) {
                //                throw new System.Exception("Resource load failed on: " + id);
            }
            return resource;
#else
            Object resource = Resources.Load(id, type);
            if (resource == null)
            {
                //                throw new System.Exception("Resource load failed on: " + id);
            }
            return resource;
#endif
        }

        public static T Load<T>(string id) where T : Object
        {
            if (string.IsNullOrEmpty(id))
                return null;
#if DOWNLOADABLE_ASSET_BUNDLES
            T resource = null;
            if (InAssetBundle.IsMatch(id))
                resource = Resources.Load<T>(id);
            else {
                AssetBundle bundle = LoadAssetBundle(id);
                if (bundle != null)
                    resource = bundle.mainAsset as T;
            }

            if (resource == null) {
                //                throw new System.Exception("Resource load failed on: " + id);
            }
            return resource;
#else
            T resource = Resources.Load<T>(id);
            if (resource == null)
            {
                //                throw new System.Exception("Resource load failed on: " + id);
            }
            return resource;
#endif
        }

        public static Object[] LoadAll(string id)
        {
            throw new System.NotImplementedException();
        }



        public static IEnumerator ExtractArchive(DecompressInfo decompressInfo, long requiredFreeSpace)
        {
#if DOWNLOADABLE_ASSET_BUNDLES
            decompressInfo.Progress = 0;

            for (int i = 0; i < Package.ZipArchiveKeys.Count; i++) {
                string key = Package.ZipArchiveKeys[i];
                if (Unpacked.IsMatch(key)) {
                    Package.ExtractableZipArchiveKeys.Add(key);
                }
            }

            string path = Path.Combine(Package.ResourceDir, Package.ExtractableZipArchiveKeys[Package.ExtractableZipArchiveKeys.Count - 1]);
            JinkeGroup.Util.Logger.DebugT(Tag, "Checking file marker: {0}", path);
            if (JKFile.Exists(path)) {
                JinkeGroup.Util.Logger.DebugT(Tag, "JKFile.Exists");
                decompressInfo.Progress = 1.0f;
                decompressInfo.Done = true;
                yield break;
            }

            yield return null;

            O7Directory.Delete(Package.ResourceDir, true);

            // check size
            long freeSpace = AppPlugin.GetDiskFreeSpace();
            JinkeGroup.Util.Logger.DebugT(Tag, "DiskFreeSpace: {0} required {1}", freeSpace, requiredFreeSpace);
            if (freeSpace < requiredFreeSpace) {
                decompressInfo.FailedDecompressing = true;
                yield break;
            }

            float dp = 1.0f / Package.ExtractableZipArchiveKeys.Count;
            for (int i = 0; i < Package.ExtractableZipArchiveKeys.Count; i++) {
                string key = Package.ExtractableZipArchiveKeys[i];
                bool succeded = DecompressAssetBundle(key);
                if (!succeded) {
                    string filePath = Path.Combine(Package.ResourceDir, key);
                    decompressInfo.FailedDecompressing = true;
                    if (JKFile.Exists(filePath)) {
                        JKFile.Delete(filePath);
                    }
                    yield break;
                } else {
                    if (i % 4 == 0)
                        yield return null;
                }
                decompressInfo.Progress += dp;
            }
#endif
            decompressInfo.Progress = 1.0f;
            decompressInfo.Done = true;
            yield break;
        }

#if DOWNLOADABLE_ASSET_BUNDLES
        public static Object[] LoadAll(System.Type type) {
            throw new System.NotImplementedException();
        }

        private static void SetupArchiveForReading() {
            Stream zipStream = new FileStream(Package.BundleDiskPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 128);
            Package.ZipArchive = ZipArchive.Open(zipStream);
            foreach (ZipArchiveEntry entry in Package.ZipArchive.Entries) {
                JinkeGroup.Util.Logger.DebugT(Tag, "Entry: {0}", entry.Key);
                Package.ZipArchiveEntries.Add(entry.Key, entry);
                Package.ZipArchiveKeys.Add(entry.Key);
            }
        }

        public static AssetBundle LoadAssetBundle(string id) {
#if UNITY_5
            id = id.ToLowerInvariant();
#endif
            if (LoadedAssetBundles.ContainsKey(id)) {
                JinkeGroup.Util.Logger.DebugT(Tag, "bundle already loaded: {0}", id);
                return LoadedAssetBundles[id];
            }

            if (!Package.ZipArchiveEntries.ContainsKey(id)) {
                JinkeGroup.Util.Logger.WarnT(Tag, "No asset in archive with key {0}", id);
                return null;
            }

            bool loadUncompressed = Unpacked.IsMatch(id) && !FailedDecompressedBundles.Contains(id);
            AssetBundle assetBundle = null;

            if (loadUncompressed) {
                assetBundle = LoadFromUncompressedFile(id);

                if (assetBundle == null) {
                    JinkeGroup.Util.Logger.WarnT(Tag, "Uncompressed asset load fail {0}. Trying to uncompress", id);
                    DecompressAssetBundle(id);//Fallback if the user deletes the file

                    if (FailedDecompressedBundles.Contains(id)) {//If it failed to get decompressed just load it from archive
                        assetBundle = LoadFromArchive(id);
                    } else {
                        assetBundle = LoadFromUncompressedFile(id);
                    }

                    if (assetBundle == null)
                        throw new System.Exception("Failed loading asset bundle from uncompressed file and the archive: " + id);
                }
            } else {
                assetBundle = LoadFromArchive(id);
            }

            if (assetBundle != null)
                LoadedAssetBundles[id] = assetBundle;

            return assetBundle;
        }

        private static AssetBundle LoadFromUncompressedFile(string id) {
            string path = Path.Combine(Package.ResourceDir, id);
            JinkeGroup.Util.Logger.DebugT(Tag, "bundle from file {0}", path);
            return AssetBundle.CreateFromFile(path);
        }

        private static AssetBundle LoadFromArchive(string id) {
            using (Stream readStream = Package.ZipArchiveEntries[id].OpenEntryStream()) {
                byte[] archiveDecompressBuffer = new byte[readStream.Length];

                int bytesRead = readStream.Read(archiveDecompressBuffer, 0, archiveDecompressBuffer.Length);//Won't try catch anything... want it to die currently
                AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(archiveDecompressBuffer);
                if (assetBundle == null)
                    throw new System.Exception("Failed loading asset bundle: " + id);//TODO someone tuched the archive... maybe delete it so it will get redownlaoded

                JinkeGroup.Util.Logger.DebugT(Tag, "bundle from zip {0} {1}", assetBundle == null, id);
                return  assetBundle;
            }
        }

        private static bool DecompressAssetBundle(string id) {
            try {
                using (Stream readStream = Package.ZipArchiveEntries[id].OpenEntryStream()) {
                    byte[] binary = new byte[readStream.Length];
                    int bytesRead = readStream.Read(binary, 0, binary.Length);//Won't try catch anything... want it to die currently

                    string path = Path.Combine(Package.ResourceDir, id);
                    string dirname = Path.GetDirectoryName(path);

                    if (!Directory.Exists(dirname)) {
                        Directory.CreateDirectory(dirname);
                    }
                    JinkeGroup.Util.Logger.DebugT(Tag, "Decompressed: {0}", path);
                    JKFile.WriteAllBytes(path, binary);
                }
            } catch (System.Exception e) {
                FailedDecompressedBundles.Add(id);
                return false;
            }

            return true;
        }
#endif

        public static ResourceRequest LoadAsync(string id, System.Type type)
        {
            throw new System.NotImplementedException();
        }

    }
}
