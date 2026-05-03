using System.Collections.Generic;
using ChopChop.App;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace ChopChop.Editor
{
    [InitializeOnLoad]
    public static class ChopChopLocalizationTableBuilder
    {
        private const string TableName = "ChopChopText";
        private const string LocaleDirectory = "Assets/Localization/Locales";
        private const string TableDirectory = "Assets/Localization/StringTables";

        static ChopChopLocalizationTableBuilder()
        {
            EditorApplication.delayCall += RebuildIfMissing;
        }

        [MenuItem("ChopChop/Localization/Rebuild String Tables")]
        public static void Rebuild()
        {
            EnsureDirectory("Assets/Localization");
            EnsureDirectory(LocaleDirectory);
            EnsureDirectory(TableDirectory);

            var japanese = EnsureLocale("ja", SystemLanguage.Japanese, "Japanese");
            var english = EnsureLocale("en", SystemLanguage.English, "English");
            var locales = new List<Locale> { japanese, english };
            var collection = LocalizationEditorSettings.GetStringTableCollection(TableName)
                ?? LocalizationEditorSettings.CreateStringTableCollection(TableName, TableDirectory, locales);

            var japaneseTable = collection.GetTable("ja") as StringTable;
            var englishTable = collection.GetTable("en") as StringTable;
            if (japaneseTable == null)
            {
                japaneseTable = collection.AddNewTable(japanese.Identifier) as StringTable;
            }

            if (englishTable == null)
            {
                englishTable = collection.AddNewTable(english.Identifier) as StringTable;
            }

            ApplyEntries(japaneseTable, LocalizationCatalog.Japanese);
            ApplyEntries(englishTable, LocalizationCatalog.English);

            LocalizationEditorSettings.SetPreloadTableFlag(japaneseTable, true);
            LocalizationEditorSettings.SetPreloadTableFlag(englishTable, true);
            EditorUtility.SetDirty(japaneseTable);
            EditorUtility.SetDirty(englishTable);
            EditorUtility.SetDirty(japaneseTable.SharedData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RebuildIfMissing()
        {
            if (LocalizationEditorSettings.GetStringTableCollection(TableName) != null)
            {
                return;
            }

            Rebuild();
        }

        private static Locale EnsureLocale(string code, SystemLanguage language, string assetName)
        {
            var locale = LocalizationEditorSettings.GetLocale(code);
            if (locale != null)
            {
                return locale;
            }

            var path = $"{LocaleDirectory}/{assetName}.asset";
            locale = AssetDatabase.LoadAssetAtPath<Locale>(path);
            if (locale == null)
            {
                locale = Locale.CreateLocale(language);
                AssetDatabase.CreateAsset(locale, path);
            }

            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }

        private static void ApplyEntries(StringTable table, IReadOnlyDictionary<string, string> entries)
        {
            foreach (var entry in entries)
            {
                var tableEntry = table.AddEntry(entry.Key, entry.Value);
                tableEntry.Value = entry.Value;
            }
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var slash = path.LastIndexOf('/');
            var parent = path.Substring(0, slash);
            var child = path.Substring(slash + 1);
            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, child);
        }

    }
}
