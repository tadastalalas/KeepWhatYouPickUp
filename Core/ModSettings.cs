using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using KeepWhatYouPickUp.Data;
using KeepWhatYouPickUp.Diagnostics;

namespace KeepWhatYouPickUp.Core;

// The only place mod code reads settings from. MCM loaded -> MCM. MCM not loaded -> ModConfiguration.xml.
internal static class ModSettings
{
    private const string McmAssemblyName = "MCMv5";

    private static ModConfiguration _xml = new();
    private static XmlSerializer? _serializer;
    private static bool _useMcm;

    internal static bool Enabled => _useMcm ? McmEnabled() : _xml.General.Enabled;

    internal static HeroScope Scope => _useMcm ? McmScope() : _xml.General.Scope;

    internal static bool EnforceSkillRequirement => _useMcm ? McmEnforceSkillRequirement() : _xml.General.EnforceSkillRequirement;

    internal static bool HighlightUnusable => _useMcm ? McmHighlightUnusable() : _xml.General.HighlightUnusable;

    internal static bool BlockUnusablePickup => _useMcm ? McmBlockUnusablePickup() : _xml.General.BlockUnusablePickup;

    internal static bool LoseSpentWeapons => _useMcm ? McmLoseSpentWeapons() : _xml.General.LoseSpentWeapons;

    internal static bool PersistSceneDrops => _useMcm ? McmPersistSceneDrops() : _xml.DroppedItems.PersistSceneDrops;

    // Percent per in-game day (0..100).
    internal static float StolenChance => _useMcm ? McmStolenChance() : _xml.DroppedItems.StolenChance;

    internal static bool LoggingEnabled => _useMcm ? McmLoggingEnabled() : _xml.TechnicalSettings.LoggingEnabled;

    private static XmlSerializer Serializer => _serializer ??= new XmlSerializer(typeof(ModConfiguration));

    // Called on every game start (campaign and custom battle): the pickup block also runs outside the campaign.
    internal static void Reload()
    {
        _useMcm = false;
        _xml = new ModConfiguration();

        if (IsMcmLoaded())
        {
            if (McmHasSettings())
            {
                _useMcm = true;
                return;
            }

            KwypuLog.Warn("MCM is loaded but returned no settings for this mod. Using " + ModInfo.ConfigurationFileName + " instead.");
        }

        _xml = LoadFromFile();
        _xml.Clamp();
    }

    private static bool IsMcmLoaded()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            if (assemblies[i].GetName().Name == McmAssemblyName) return true;
        }

        return false;
    }

    // Same mapping as the dropdown always had: 0 = player, 1 = player clan, anything else = all heroes.
    private static HeroScope ToScope(int index) => index switch
    {
        0 => HeroScope.PlayerOnly,
        1 => HeroScope.PlayerAndClan,
        _ => HeroScope.AllHeroes,
    };

    // Every method that touches MCMSettings must be NoInlining and may only be called when MCM is loaded.
    // Otherwise the game tries to load MCM's classes and crashes when MCM is not installed.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmHasSettings() => MCMSettings.Instance != null;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmEnabled() => MCMSettings.Instance?.Enabled ?? ModConfiguration.EnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static HeroScope McmScope() => MCMSettings.Instance?.Scope is { } scope ? ToScope(scope.SelectedIndex) : ModConfiguration.ScopeDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmEnforceSkillRequirement() => MCMSettings.Instance?.EnforceSkillRequirement ?? ModConfiguration.EnforceSkillRequirementDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmHighlightUnusable() => MCMSettings.Instance?.HighlightUnusable ?? ModConfiguration.HighlightUnusableDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmBlockUnusablePickup() => MCMSettings.Instance?.BlockUnusablePickup ?? ModConfiguration.BlockUnusablePickupDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmLoseSpentWeapons() => MCMSettings.Instance?.LoseSpentWeapons ?? ModConfiguration.LoseSpentWeaponsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmPersistSceneDrops() => MCMSettings.Instance?.PersistSceneDrops ?? ModConfiguration.PersistSceneDropsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float McmStolenChance() => MCMSettings.Instance?.StolenChance ?? ModConfiguration.StolenChanceDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmLoggingEnabled() => MCMSettings.Instance?.LoggingEnabled ?? ModConfiguration.LoggingEnabledDefault;

    private static ModConfiguration LoadFromFile()
    {
        string path = Path.Combine(ModInfo.ModuleRoot, ModInfo.ConfigurationFileName);
        ModConfiguration configuration = new();

        if (!File.Exists(path))
        {
            KwypuLog.Warn(ModInfo.ConfigurationFileName + " was missing. Created it with the default settings.");
            TryWrite(path, configuration);
            return configuration;
        }

        try
        {
            configuration = Read(path);
            if (!HasEverySetting(path, configuration))
            {
                KwypuLog.Warn(ModInfo.ConfigurationFileName + " had missing settings. Added them with their default values.");
                TryWrite(path, configuration);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or XmlException)
        {
            KwypuLog.Warn(ModInfo.ConfigurationFileName + " was broken and was replaced with the default settings. Reason: " + (ex.InnerException?.Message ?? ex.Message));
            configuration = new ModConfiguration();
            TryWrite(path, configuration);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            KwypuLog.Warn(ModInfo.ConfigurationFileName + " could not be opened. Using the default settings. Reason: " + ex.Message);
            configuration = new ModConfiguration();
        }

        return configuration;
    }

    private static ModConfiguration Read(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Serializer.Deserialize(stream) as ModConfiguration ?? new ModConfiguration();
    }

    private static void TryWrite(string path, ModConfiguration configuration)
    {
        try
        {
            XmlWriterSettings writerSettings = new() { Indent = true, Encoding = Encoding.UTF8 };
            using XmlWriter writer = XmlWriter.Create(path, writerSettings);
            Serializer.Serialize(writer, configuration, NoNamespaces());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            KwypuLog.Warn(ModInfo.ConfigurationFileName + " could not be written. The settings are kept in memory for this session. Reason: " + ex.Message);
        }
    }

    private static bool HasEverySetting(string path, ModConfiguration configuration)
    {
        XDocument complete = new();
        using (XmlWriter writer = complete.CreateWriter())
        {
            Serializer.Serialize(writer, configuration, NoNamespaces());
        }

        XElement? fileRoot = XDocument.Load(path).Root;
        return fileRoot != null && complete.Root != null && ContainsEveryElement(fileRoot, complete.Root);
    }

    private static bool ContainsEveryElement(XElement actual, XElement expected)
    {
        foreach (XElement expectedChild in expected.Elements())
        {
            XElement? actualChild = actual.Element(expectedChild.Name);
            if (actualChild == null || !ContainsEveryElement(actualChild, expectedChild)) return false;
        }

        return true;
    }

    private static XmlSerializerNamespaces NoNamespaces()
    {
        XmlSerializerNamespaces namespaces = new();
        namespaces.Add(string.Empty, string.Empty);
        return namespaces;
    }
}
