﻿using System.Text.Json;
using DemoFile;
using CS2Nades.Console.JsonConverters;
using CS2Nades.Common;

var pathArgument = args.SingleOrDefault();

if (string.IsNullOrEmpty(pathArgument))
{
    Console.Error.WriteLine("Wrong argument: path to a demo file.");
    Environment.Exit(1);
}

var path = pathArgument.Normalize();

if (!File.Exists(path))
{
    Console.Error.WriteLine($"File doesn't exist: {path}.");
    Environment.Exit(1);
}

var demoParser = new DemoParser();
var nadesHandler = new NadesHandler(demoParser);

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

jsonOptions.Converters.Add(new InputButtonsJsonConverter());
jsonOptions.Converters.Add(new GameTickJsonConverter());
jsonOptions.Converters.Add(new GameTimeJsonConverter());

nadesHandler.OnResult += nade =>
{
    Console.Out.WriteLine(JsonSerializer.Serialize(nade, jsonOptions));
};

try
{
    await demoParser.ReadAllAsync(File.OpenRead(path));
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.Exit(1);
}