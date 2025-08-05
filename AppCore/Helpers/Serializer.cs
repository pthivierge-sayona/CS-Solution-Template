#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

using System.Text.Json;

namespace NewApp.Core.Helpers;

/// <summary>
/// Modern JSON-based serialization helper using System.Text.Json
/// </summary>
public static class Serializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serialize an object to JSON and save to file
    /// </summary>
    /// <param name="filename">File path to save to</param>
    /// <param name="objectToSerialize">Object to serialize</param>
    /// <param name="options">Optional JSON serializer options</param>
    public static async Task SerializeObjectAsync(string filename, object objectToSerialize, JsonSerializerOptions? options = null)
    {
        options ??= DefaultOptions;
        
        var json = JsonSerializer.Serialize(objectToSerialize, options);
        await File.WriteAllTextAsync(filename, json);
    }

    /// <summary>
    /// Synchronous version of SerializeObject for backward compatibility
    /// </summary>
    /// <param name="filename">File path to save to</param>
    /// <param name="objectToSerialize">Object to serialize</param>
    /// <param name="options">Optional JSON serializer options</param>
    public static void SerializeObject(string filename, object objectToSerialize, JsonSerializerOptions? options = null)
    {
        options ??= DefaultOptions;
        
        var json = JsonSerializer.Serialize(objectToSerialize, options);
        File.WriteAllText(filename, json);
    }

    /// <summary>
    /// Deserialize an object from JSON file
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="filename">File path to read from</param>
    /// <param name="options">Optional JSON serializer options</param>
    /// <returns>Deserialized object</returns>
    public static async Task<T?> DeserializeObjectAsync<T>(string filename, JsonSerializerOptions? options = null)
    {
        options ??= DefaultOptions;
        
        if (!File.Exists(filename))
            return default;

        var json = await File.ReadAllTextAsync(filename);
        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// Synchronous version of DeserializeObject for backward compatibility
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="filename">File path to read from</param>
    /// <param name="options">Optional JSON serializer options</param>
    /// <returns>Deserialized object</returns>
    public static T? DeserializeObject<T>(string filename, JsonSerializerOptions? options = null)
    {
        options ??= DefaultOptions;
        
        if (!File.Exists(filename))
            return default;

        var json = File.ReadAllText(filename);
        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// Legacy method for backward compatibility - returns object type
    /// </summary>
    /// <param name="filename">File path to read from</param>
    /// <returns>Deserialized object as object type</returns>
    [Obsolete("Use generic DeserializeObject<T> method instead")]
    public static object? DeserializeObject(string filename)
    {
        if (!File.Exists(filename))
            return null;

        var json = File.ReadAllText(filename);
        return JsonSerializer.Deserialize<object>(json, DefaultOptions);
    }
}