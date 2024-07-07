﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LinuxDesktopUtils.XDGDesktopPortal;

namespace LinuxDesktopUtils.Example;

[SuppressMessage("Usage", "MA0004:Use Task.ConfigureAwait")]
public static class Program
{
    public static async Task Main(string[] args)
    {
        await RunAsync();
        Console.WriteLine("done");
    }

    private static async Task RunAsync()
    {
        var path = Path.Combine(Path.GetDirectoryName(Path.Combine(Environment.ProcessPath ?? "")) ?? "", "tmp.txt");
        var pathDirectory = Path.GetDirectoryName(path)!;
        await File.WriteAllTextAsync(path, "hello world");

        await using var connectionManager = await DesktopPortalConnectionManager.ConnectAsync();

        using var cts = new CancellationTokenSource();
        using var _ = PosixSignalRegistration.Create(PosixSignal.SIGINT, _ => cts.Cancel());

        try
        {
            var portal = await connectionManager.GetFileChooserPortalAsync();
            var response = await portal.OpenFileAsync(
                dialogTitle: "Choose one or more files",
                options: new FileChooser.OpenFileOptions
                {
                    AcceptLabel = "I choose you!",
                    AllowMultiple = true,
                    SelectDirectories = false,
                    IsDialogModal = true,
                    SuggestedFolder = DirectoryPath.From(pathDirectory),
                    Filters = [
                        new FileChooser.OpenFileFilter
                        {
                            FilterName = "Images",
                            Patterns =
                            [
                                GlobPattern.From("*.ico"),
                                MimeType.From("image/png"),
                            ],
                        },
                        new FileChooser.OpenFileFilter
                        {
                            FilterName = "Text",
                            Patterns = [
                                GlobPattern.From("*.txt"),
                            ],
                            IsDefault = true,
                        },
                    ],
                    Choices = [
                        new FileChooser.OpenFileComboBox
                        {
                            Id = "encoding",
                            Label = "Encoding",
                            Choices = [
                                new FileChooser.OpenFileChoice
                                {
                                    Id = "utf8",
                                    Label = "Unicode (UTF-8)",
                                    IsDefault = true,
                                },
                                new FileChooser.OpenFileChoice
                                {
                                    Id = "latin15",
                                    Label = "Western",
                                },
                            ],
                        },
                        new FileChooser.OpenFileCheckBox
                        {
                            Id = "reencode",
                            Label = "Reencode",
                            DefaultValue = false,
                        },
                    ],
                },
                cancellationToken: cts.Token
            );

            Console.WriteLine(response.Status);
            if (response.Status == ResponseStatus.Success)
            {
                foreach (var selectedFile in response.Results.Value.SelectedFiles)
                {
                    Console.WriteLine($"Selected: {selectedFile}");
                }

                var first = response.Results.Value.SelectedFiles[0];
                var openUriPortal = await connectionManager.GetOpenUriPortalAsync();
                await openUriPortal.OpenFileAsync(
                    file: first,
                    cancellationToken: cts.Token
                );
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
