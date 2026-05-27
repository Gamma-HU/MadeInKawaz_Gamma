using UnityEditor;
using UnityEngine;

using System;
using System.IO;
using System.Reflection;
using System.Text;

public sealed class AudioClipVolumeAdjusterWindow : EditorWindow
{
    private AudioClip sourceClip;
    private AudioClip previewClip;

    private float gainDb = 0.0f;
    private bool preventClipping = true;
    private bool loopPreview = false;

    private float[] processedSamples;
    private float peakBefore;
    private float peakAfter;
    private float appliedLinearGain = 1.0f;

    private bool previewDirty = true;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Audio/Audio Clip Volume Adjuster")]
    private static void Open()
    {
        AudioClipVolumeAdjusterWindow window = GetWindow<AudioClipVolumeAdjusterWindow>();
        window.titleContent = new GUIContent("Audio Volume");
        window.minSize = new Vector2(420.0f, 360.0f);
        window.TryAssignSelection();
        window.Show();
    }

    private void OnEnable()
    {
        TryAssignSelection();
    }

    private void OnDisable()
    {
        StopPreview();

        if (previewClip != null)
        {
            DestroyImmediate(previewClip);
            previewClip = null;
        }
    }

    private void OnSelectionChange()
    {
        TryAssignSelection();
        Repaint();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(8.0f);

        EditorGUI.BeginChangeCheck();

        sourceClip = (AudioClip)EditorGUILayout.ObjectField(
            "Source Clip",
            sourceClip,
            typeof(AudioClip),
            false
        );

        EditorGUILayout.Space(6.0f);

        gainDb = EditorGUILayout.Slider("Gain dB", gainDb, -60.0f, 24.0f);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("-6 dB"))
            {
                gainDb -= 6.0f;
                previewDirty = true;
            }

            if (GUILayout.Button("-3 dB"))
            {
                gainDb -= 3.0f;
                previewDirty = true;
            }

            if (GUILayout.Button("0 dB"))
            {
                gainDb = 0.0f;
                previewDirty = true;
            }

            if (GUILayout.Button("+3 dB"))
            {
                gainDb += 3.0f;
                previewDirty = true;
            }

            if (GUILayout.Button("+6 dB"))
            {
                gainDb += 6.0f;
                previewDirty = true;
            }
        }

        preventClipping = EditorGUILayout.ToggleLeft("ピークが1.0を超える場合は自動で下げる", preventClipping);
        loopPreview = EditorGUILayout.ToggleLeft("ループ再生", loopPreview);

        if (EditorGUI.EndChangeCheck())
        {
            previewDirty = true;
        }

        EditorGUILayout.Space(10.0f);

        DrawClipInfo();

        EditorGUILayout.Space(10.0f);

        using (new EditorGUI.DisabledScope(sourceClip == null))
        {
            if (GUILayout.Button("変更後クリップを生成 / 更新", GUILayout.Height(28.0f)))
            {
                BuildPreviewClip();
            }
        }

        EditorGUILayout.Space(6.0f);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(sourceClip == null))
            {
                if (GUILayout.Button("元の音を再生", GUILayout.Height(28.0f)))
                {
                    PlayClip(sourceClip, loopPreview);
                }
            }

            using (new EditorGUI.DisabledScope(sourceClip == null))
            {
                if (GUILayout.Button("変更後をプレビュー", GUILayout.Height(28.0f)))
                {
                    if (previewDirty || previewClip == null)
                    {
                        BuildPreviewClip();
                    }

                    if (previewClip != null)
                    {
                        PlayClip(previewClip, loopPreview);
                    }
                }
            }

            if (GUILayout.Button("停止", GUILayout.Height(28.0f)))
            {
                StopPreview();
            }
        }

        EditorGUILayout.Space(12.0f);

        using (new EditorGUI.DisabledScope(sourceClip == null))
        {
            if (GUILayout.Button("変更後のWAVを保存", GUILayout.Height(32.0f)))
            {
                SaveAdjustedWav();
            }
        }

        EditorGUILayout.Space(10.0f);

        DrawWarnings();

        EditorGUILayout.EndScrollView();
    }

    private void TryAssignSelection()
    {
        AudioClip selectedClip = Selection.activeObject as AudioClip;

        if (selectedClip != null && selectedClip != sourceClip)
        {
            sourceClip = selectedClip;
            previewDirty = true;
        }
    }

    private void DrawClipInfo()
    {
        if (sourceClip == null)
        {
            EditorGUILayout.HelpBox(
                "AudioClipを指定してください。ProjectビューでAudioClipを選択してからこのウィンドウを開くこともできます。",
                MessageType.Info
            );

            return;
        }

        float requestedLinearGain = DbToLinear(gainDb);

        EditorGUILayout.LabelField("Clip Name", sourceClip.name);
        EditorGUILayout.LabelField("Samples", sourceClip.samples.ToString());
        EditorGUILayout.LabelField("Channels", sourceClip.channels.ToString());
        EditorGUILayout.LabelField("Frequency", sourceClip.frequency + " Hz");
        EditorGUILayout.LabelField("Length", sourceClip.length.ToString("0.000") + " sec");
        EditorGUILayout.LabelField("Requested Gain", requestedLinearGain.ToString("0.000") + " x");

        if (processedSamples != null && !previewDirty)
        {
            EditorGUILayout.Space(4.0f);
            EditorGUILayout.LabelField("Peak Before", peakBefore.ToString("0.000"));
            EditorGUILayout.LabelField("Peak After", peakAfter.ToString("0.000"));
            EditorGUILayout.LabelField("Applied Gain", appliedLinearGain.ToString("0.000") + " x");
        }
    }

    private void DrawWarnings()
    {
        if (sourceClip == null)
        {
            return;
        }

        EditorGUILayout.HelpBox(
            "圧縮されたAudioClipで読み取りに失敗する場合は、Import SettingsのLoad Typeを Decompress On Load にしてください。",
            MessageType.Warning
        );

        if (previewDirty)
        {
            EditorGUILayout.HelpBox(
                "現在の設定はまだプレビュー用クリップに反映されていません。「変更後クリップを生成 / 更新」または「変更後をプレビュー」を押してください。",
                MessageType.None
            );
        }
    }

    private void BuildPreviewClip()
    {
        if (sourceClip == null)
        {
            EditorUtility.DisplayDialog("Audio Clip Volume Adjuster", "Source Clipが指定されていません。", "OK");
            return;
        }

        if (!sourceClip.LoadAudioData())
        {
            EditorUtility.DisplayDialog(
                "Audio Clip Volume Adjuster",
                "AudioClipのロードに失敗しました。",
                "OK"
            );

            return;
        }

        int totalSampleCount = sourceClip.samples * sourceClip.channels;
        float[] sourceSamples = new float[totalSampleCount];

        bool succeeded = sourceClip.GetData(sourceSamples, 0);

        if (!succeeded)
        {
            EditorUtility.DisplayDialog(
                "Audio Clip Volume Adjuster",
                "AudioClipのサンプルデータを取得できませんでした。\n\nImport SettingsのLoad Typeを Decompress On Load にしてください。",
                "OK"
            );

            return;
        }

        processedSamples = new float[totalSampleCount];

        float requestedGain = DbToLinear(gainDb);
        peakBefore = GetPeak(sourceSamples);

        float expectedPeak = peakBefore * requestedGain;
        appliedLinearGain = requestedGain;

        if (preventClipping && expectedPeak > 1.0f)
        {
            appliedLinearGain = requestedGain / expectedPeak;
        }

        for (int i = 0; i < sourceSamples.Length; i++)
        {
            processedSamples[i] = Mathf.Clamp(sourceSamples[i] * appliedLinearGain, -1.0f, 1.0f);
        }

        peakAfter = GetPeak(processedSamples);

        if (previewClip != null)
        {
            DestroyImmediate(previewClip);
            previewClip = null;
        }

        previewClip = AudioClip.Create(
            sourceClip.name + "_VolumePreview",
            sourceClip.samples,
            sourceClip.channels,
            sourceClip.frequency,
            false
        );

        bool setSucceeded = previewClip.SetData(processedSamples, 0);

        if (!setSucceeded)
        {
            EditorUtility.DisplayDialog(
                "Audio Clip Volume Adjuster",
                "プレビュー用AudioClipへの書き込みに失敗しました。",
                "OK"
            );

            DestroyImmediate(previewClip);
            previewClip = null;
            return;
        }

        previewDirty = false;
        Repaint();
    }

    private void SaveAdjustedWav()
    {
        if (sourceClip == null)
        {
            EditorUtility.DisplayDialog("Audio Clip Volume Adjuster", "Source Clipが指定されていません。", "OK");
            return;
        }

        if (previewDirty || processedSamples == null)
        {
            BuildPreviewClip();
        }

        if (processedSamples == null)
        {
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(sourceClip);
        string sourceDirectory = "Assets";
        string defaultName = sourceClip.name + "_volume_adjusted.wav";

        if (!string.IsNullOrEmpty(sourcePath))
        {
            string directory = Path.GetDirectoryName(sourcePath);

            if (!string.IsNullOrEmpty(directory))
            {
                sourceDirectory = directory.Replace("\\", "/");
            }
        }

        string savePath = EditorUtility.SaveFilePanelInProject(
            "変更後のWAVを保存",
            defaultName,
            "wav",
            "保存先を指定してください。",
            sourceDirectory
        );

        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }

        WriteWavFile(
            savePath,
            processedSamples,
            sourceClip.channels,
            sourceClip.frequency
        );

        AssetDatabase.ImportAsset(savePath);
        AssetDatabase.Refresh();

        AudioClip savedClip = AssetDatabase.LoadAssetAtPath<AudioClip>(savePath);

        if (savedClip != null)
        {
            Selection.activeObject = savedClip;
            EditorGUIUtility.PingObject(savedClip);
        }

        EditorUtility.DisplayDialog(
            "Audio Clip Volume Adjuster",
            "保存しました。\n" + savePath,
            "OK"
        );
    }

    private static float DbToLinear(float db)
    {
        return Mathf.Pow(10.0f, db / 20.0f);
    }

    private static float GetPeak(float[] samples)
    {
        float peak = 0.0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float value = Mathf.Abs(samples[i]);

            if (value > peak)
            {
                peak = value;
            }
        }

        return peak;
    }

    private static void WriteWavFile(
        string assetRelativePath,
        float[] samples,
        int channels,
        int frequency
    )
    {
        string fullPath = Path.GetFullPath(assetRelativePath);

        string directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        const int bitsPerSample = 16;
        const int bytesPerSample = bitsPerSample / 8;

        int dataSize = samples.Length * bytesPerSample;
        int byteRate = frequency * channels * bytesPerSample;
        short blockAlign = (short)(channels * bytesPerSample);

        using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(fileStream, Encoding.UTF8))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(frequency);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write((short)bitsPerSample);

            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            for (int i = 0; i < samples.Length; i++)
            {
                float clamped = Mathf.Clamp(samples[i], -1.0f, 1.0f);
                short intSample = (short)Mathf.RoundToInt(clamped * short.MaxValue);
                writer.Write(intSample);
            }
        }
    }

    private static void PlayClip(AudioClip clip, bool loop)
    {
        if (clip == null)
        {
            return;
        }

        StopPreview();

        Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

        if (audioUtilType == null)
        {
            EditorUtility.DisplayDialog(
                "Audio Clip Volume Adjuster",
                "UnityEditor.AudioUtilが見つかりませんでした。",
                "OK"
            );

            return;
        }

        MethodInfo playPreviewClipMethod = audioUtilType.GetMethod(
            "PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null
        );

        if (playPreviewClipMethod != null)
        {
            playPreviewClipMethod.Invoke(null, new object[] { clip, 0, loop });
            return;
        }

        MethodInfo playClipMethod = audioUtilType.GetMethod(
            "PlayClip",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(AudioClip) },
            null
        );

        if (playClipMethod != null)
        {
            playClipMethod.Invoke(null, new object[] { clip });
            return;
        }

        EditorUtility.DisplayDialog(
            "Audio Clip Volume Adjuster",
            "このUnityバージョンではエディタ上の音声プレビューAPIを呼び出せませんでした。",
            "OK"
        );
    }

    private static void StopPreview()
    {
        Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

        if (audioUtilType == null)
        {
            return;
        }

        MethodInfo stopAllPreviewClipsMethod = audioUtilType.GetMethod(
            "StopAllPreviewClips",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null
        );

        if (stopAllPreviewClipsMethod != null)
        {
            stopAllPreviewClipsMethod.Invoke(null, null);
            return;
        }

        MethodInfo stopAllClipsMethod = audioUtilType.GetMethod(
            "StopAllClips",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null
        );

        if (stopAllClipsMethod != null)
        {
            stopAllClipsMethod.Invoke(null, null);
        }
    }
}