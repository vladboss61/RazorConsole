using ConsoleApp.Render.assets;
using ConsoleApp.Render.Core;
using ConsoleApp.Render.Core.Interfaces;
using ConsoleApp.Render.Models.ViewModels;
using ConsoleApp.Render.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleApp.RazorDependency;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackAsAttribute : Attribute
{
    public string? Domain { get; }
    public string FriendlyName { get; }

    public TrackAsAttribute(string friendlyName)
    {
        FriendlyName = friendlyName;
    }

    public TrackAsAttribute(string domain, string friendlyName)
    {
        Domain = domain;
        FriendlyName = friendlyName;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackIdAttribute : Attribute
{
}

public static class TrackComparer
{
    public static List<TrackChange> Compare<T>(T? oldVersion, T? newVersion, string? parentPath = null, string? parentSubDomain = null) where T : class
    {
        var changes = new List<TrackChange>();

        if (oldVersion == null && newVersion == null)
        {
            return changes;
        }

        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var trackAttribute = property.GetCustomAttribute<TrackAsAttribute>();
            if (trackAttribute == null)
            {
                continue;
            }

            var oldValue = oldVersion != null ? property.GetValue(oldVersion) : null;
            var newValue = newVersion != null ? property.GetValue(newVersion) : null;

            var currentPath = string.IsNullOrEmpty(parentPath)
                ? property.Name
                : $"{parentPath}.{property.Name}";

            var friendlyNameWithSubdomain = string.IsNullOrEmpty(trackAttribute.Domain)
                ? trackAttribute.FriendlyName
                : $"{trackAttribute.Domain}.{trackAttribute.FriendlyName}";

            var currentFriendlyName = string.IsNullOrEmpty(parentPath)
                ? friendlyNameWithSubdomain
                : $"{parentPath}.{friendlyNameWithSubdomain}";

            var currentDomain = string.IsNullOrEmpty(parentSubDomain) ? trackAttribute.Domain : parentSubDomain;

            if (IsCollection(property.PropertyType))
            {
                var collectionChanges = CompareCollections(oldValue, newValue, property.PropertyType, currentPath, currentFriendlyName, currentDomain);
                changes.AddRange(collectionChanges);
            }
            else if (IsComplexType(property.PropertyType) && HasTrackAsAttribute(property.PropertyType))
            {
                // Recursively compare nested objects
                var nestedChanges = CompareNested(oldValue, newValue, property.PropertyType, currentFriendlyName, currentDomain);
                changes.AddRange(nestedChanges);
            }
            else if (!AreEqual(oldValue, newValue))
            {
                changes.Add(new TrackChange
                {
                    PropertyName = currentPath,
                    Domain = currentDomain,
                    FriendlyName = currentFriendlyName,
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }
        }

        return changes;
    }

    private static List<TrackChange> CompareNested(object? oldValue, object? newValue, Type type, string parentPath, string? parentDomain)
    {
        if (oldValue == null && newValue == null)
        {
            return [];
        }

        var compareMethod = typeof(TrackComparer)
            .GetMethod(nameof(Compare), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(type);

        var result = compareMethod.Invoke(null, [oldValue, newValue, parentPath, parentDomain]);
        return (List<TrackChange>)result!;
    }

    private static bool IsComplexType(Type type)
    {
        return type.IsClass && type != typeof(string) && !type.IsPrimitive;
    }

    private static bool HasTrackAsAttribute(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return properties.Any(p => p.GetCustomAttribute<TrackAsAttribute>() != null);
    }

    private static bool IsCollection(Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static List<TrackChange> CompareCollections(object? oldValue, object? newValue, Type collectionType, string currentPath, string currentFriendlyName, string? currentSubDomain)
    {
        var changes = new List<TrackChange>();

        var oldList = oldValue as IEnumerable;
        var newList = newValue as IEnumerable;

        if (oldList == null && newList == null)
        {
            return changes;
        }

        var oldItems = oldList?.Cast<object>().ToList() ?? new List<object>();
        var newItems = newList?.Cast<object>().ToList() ?? new List<object>();

        Type? elementType = null;
        if (collectionType.IsGenericType)
        {
            elementType = collectionType.GetGenericArguments()[0];
        }
        else if (collectionType.IsArray)
        {
            elementType = collectionType.GetElementType();
        }

        if (elementType != null && HasTrackAsAttribute(elementType))
        {
            var identifierProperty = GetIdentifierProperty(elementType);

            if (identifierProperty != null)
            {
                changes.AddRange(CompareCollectionsById(oldItems, newItems, elementType, identifierProperty, currentPath, currentFriendlyName, currentSubDomain));
            }
            else
            {
                changes.AddRange(CompareCollectionsByIndex(oldItems, newItems, elementType, currentPath, currentFriendlyName, currentSubDomain));
            }
        }
        else
        {
            if (!CollectionsEqual(oldItems, newItems))
            {
                changes.Add(new TrackChange
                {
                    PropertyName = currentPath,
                    Domain = currentSubDomain,
                    FriendlyName = currentFriendlyName,
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }
        }

        return changes;
    }

    private static PropertyInfo GetIdentifierProperty(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return properties.FirstOrDefault(p => p.GetCustomAttribute<TrackIdAttribute>() != null);
    }

    private static List<TrackChange> CompareCollectionsById(List<object> oldItems, List<object> newItems, Type elementType, PropertyInfo identifierProperty, string currentPath, string currentFriendlyName, string? currentSubDomain)
    {
        var changes = new List<TrackChange>();

        var oldDict = oldItems.ToDictionary(item => identifierProperty.GetValue(item)!);
        var newDict = newItems.ToDictionary(item => identifierProperty.GetValue(item)!);

        var allIdentifiers = oldDict.Keys.Union(newDict.Keys).ToList();

        foreach (var identifier in allIdentifiers)
        {
            var oldItem = oldDict.ContainsKey(identifier) ? oldDict[identifier] : null;
            var newItem = newDict.ContainsKey(identifier) ? newDict[identifier] : null;

            var itemPath = $"{currentPath}[{identifier}]";
            var itemFriendlyName = $"{currentFriendlyName}[{identifier}]";

            if (oldItem == null && newItem == null)
            {
                continue;
            }

            var itemChanges = CompareNested(oldItem, newItem, elementType, itemFriendlyName, currentSubDomain);
            changes.AddRange(itemChanges);
        }

        return changes;
    }

    private static List<TrackChange> CompareCollectionsByIndex(List<object> oldItems, List<object> newItems, Type elementType, string currentPath, string currentFriendlyName, string? currentSubDomain)
    {
        var changes = new List<TrackChange>();
        var maxCount = Math.Max(oldItems.Count, newItems.Count);

        for (int i = 0; i < maxCount; i++)
        {
            var oldItem = i < oldItems.Count ? oldItems[i] : null;
            var newItem = i < newItems.Count ? newItems[i] : null;

            var itemPath = $"{currentPath}[{i}]";
            var itemFriendlyName = $"{currentFriendlyName}[{i}]";

            if (oldItem == null && newItem == null)
            {
                continue;
            }

            var itemChanges = CompareNested(oldItem, newItem, elementType, itemFriendlyName, currentSubDomain);
            changes.AddRange(itemChanges);
        }

        return changes;
    }

    private static bool CollectionsEqual(List<object> list1, List<object> list2)
    {
        if (list1.Count != list2.Count)
        {
            return false;
        }

        for (int i = 0; i < list1.Count; i++)
        {
            if (!AreEqual(list1[i], list2[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        return left.Equals(right);
    }
}

public class TrackChange
{
    public string PropertyName { get; set; } = string.Empty;

    public string Domain { get; set; }
    
    public string FriendlyName { get; set; } = string.Empty;
    
    public object OldValue { get; set; }
    
    public object NewValue { get; set; }

    public override string ToString()
    {
        return $"{FriendlyName}: {OldValue} → {NewValue}";
    }
}


public class Information
{
    [TrackAs(domain: nameof(Information), friendlyName: "Information about changes")]
    public string Info { get; set; }

    [TrackAs(friendlyName: "InnerObjects")]
    public List<InnerModel> InnerObjects { get; set; }

    [TrackId]
    public Guid InfoId { get; set; }
}

public class InnerModel
{
    [TrackId]
    public int Id { get; set; }

    [TrackAs(domain: nameof(InnerModel), "Name")]
    public string Name { get; set; }
}

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        var guid = Guid.NewGuid();
        var oldInfo = new Information() {  Info = "Old Info", InfoId = guid };
        var newInfo = new Information() { Info = "New Info", InfoId = guid };
        var changes = TrackComparer.Compare<Information>(oldInfo, newInfo);

        changes.ForEach(change => Console.WriteLine(change.ToString()));

        IServiceCollection services = new ServiceCollection();
        services.AddRazorHtmlRenderer();

        var simpleBodyParams = new Dictionary<string, object>()
            {
                { nameof(SimpleBodyComponent.Info), "Hey simple" },
                { nameof(SimpleBodyComponent.InnerBody), Fragment.ToFragment<InnerSimpleBodyComponent>() }
            };

        //var downloadBytes = File.ReadAllBytes("./assets/download.jpg");
        string downloadImageBase64 = Convert.ToBase64String(Resources.download);

        var downloadVideoBytes = File.ReadAllBytes("./assets/download_video.mp4");
        string downloadVideoBase64 = Convert.ToBase64String(Resources.download_video);

        var dictionary = new Dictionary<string, object>
            {
                { nameof(IndexComponent.DownloadVideoBase64), downloadVideoBase64 },
                { nameof(IndexComponent.DownloadImageBase64), downloadImageBase64 },
                { nameof(IndexComponent.IsInnerApplied), false },
                { nameof(IndexComponent.Message), "Hello from the External Lib Render Message component!" },
                { nameof(IndexComponent.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(IndexComponent.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
                { nameof(IndexComponent.Header), Fragment.ToFragment<HeaderComponent>() },
                { nameof(IndexComponent.Body), Fragment.ToFragment<SimpleBodyComponent>(simpleBodyParams) }
            };

        //================= String Render =================
        var render = services.BuildServiceProvider().GetService<IHtmlRender>();
        var html = await render.RenderAsync<IndexComponent>(dictionary);
        Console.WriteLine(html);
        //================= String Render =================

        //================= Stream 1 Render =================
        var ms = new MemoryStream();
        await render.RenderStreamAsync<IndexComponent>(ms, dictionary);
        
        StreamReader reader = new StreamReader(ms);

        Console.WriteLine("==================");
        Console.WriteLine(reader.ReadToEnd());
        Console.WriteLine("==================");
        //================= Stream 1 Render =================

        //================= Stream 2 Render =================
        var dictionary2 = new Dictionary<string, object>
            {
                { nameof(IndexComponent.Message), "Message 2" },
                { nameof(IndexComponent.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(IndexComponent.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
            };

        using var html2Stream = await render.RenderStreamAsync<IndexComponent>(dictionary2);
        using var sr = new StreamReader(html2Stream);
        var s = await sr.ReadToEndAsync();
        Console.WriteLine(s);
        //================= Stream 2 Render =================
    }
}
