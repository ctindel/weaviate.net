// Copyright (C) 2023 Weaviate - https://weaviate.io
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// ReSharper disable once CheckNamespace
namespace Weaviate.Client;

public static class DataType
{
    public static string String = "string";
    public static string StringArray = "string[]";
    public static string Int = "int";
    public static string IntArray = "int[]";
    public static string Boolean = "boolean";
    public static string BooleanArray = "boolean[]";
    public static string Number = "number";
    public static string NumberArray = "number[]";
    public static string Date = "date";
    public static string DateArray = "date[]";
    public static string Text = "text";
    public static string TextArray = "text[]";
    public static string GeoCoordinates = "geoCoordinates";
    public static string PhoneNumber = "phoneNumber";
    public static string Blob = "blob";
    public static string Uuid = "uuid";
    public static string UuidArray = "uuid[]";
    public static string Object = "object";
    public static string ObjectArray = "object[]";
    public static string Cref = "cref";
}
