using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mavrodiev.Playground.CSharp
{
    // Code Watch, part three: Predict mode.
    // Switched on, it takes a baseline of the counters and hides the run marks, counts and values.
    // The student marks the lines expected to run and may type expected values. Whatever runs
    // after that is counted but not shown. Reveal compares and appends the record to
    // Assets/Predictions/CodeWatch-<ScriptName>.md.
    // The state is serialized so that it survives the script reload when Play mode starts.
    public partial class CodeWatchWindow
    {
        const string PredictionsFolder = "Assets/Predictions";

        [SerializeField] bool predictOn;
        [SerializeField] bool revealed;
        [SerializeField] bool needBaseline;
        [SerializeField] string predictScript = "";
        [SerializeField] List<int> markedLines = new List<int>();
        [SerializeField] List<string> expectedNames = new List<string>();
        [SerializeField] List<string> expectedValues = new List<string>();
        // Counters when Predict started (or at the last Play mode change), and what ran before that change.
        [SerializeField] List<int> baseLines = new List<int>();
        [SerializeField] List<int> baseCounts = new List<int>();
        [SerializeField] List<int> carriedLines = new List<int>();
        [SerializeField] List<int> carriedCounts = new List<int>();
        // What Reveal found: the lines that ran with their counts, and the actual values.
        [SerializeField] List<int> revealLines = new List<int>();
        [SerializeField] List<int> revealCounts = new List<int>();
        [SerializeField] List<string> revealNames = new List<string>();
        [SerializeField] List<string> revealValues = new List<string>();
        [SerializeField] string revealResult = "";
        [SerializeField] string revealFile = "";

        bool PredictHidesRuns
        {
            get { return predictOn && !revealed; }
        }

        void DrawPredictToolbar()
        {
            using (new EditorGUI.DisabledScope(!Ready || target == null))
            {
                bool on = GUILayout.Toggle(predictOn, "Predict", EditorStyles.toolbarButton, GUILayout.Width(56f));
                if (on && !predictOn) StartPredict();
                else if (!on && predictOn) StopPredict();
            }
            if (!predictOn) return;
            if (!revealed)
            {
                if (GUILayout.Button("Reveal", EditorStyles.toolbarButton, GUILayout.Width(52f))) Reveal();
            }
            else
            {
                if (GUILayout.Button("Again", EditorStyles.toolbarButton, GUILayout.Width(48f))) StartPredict();
            }
        }

        void DrawPredictBar()
        {
            if (!predictOn) return;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (!revealed)
                {
                    GUILayout.Label("Predict: click beside each line you expect to run. You can also type the value you expect for a variable, on the right. Then play the game or press Call, and press Reveal.", wrapStyle);
                    GUILayout.Label("Lines marked: " + markedLines.Count, EditorStyles.miniLabel);
                    return;
                }
                GUILayout.Label(revealResult, EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    Legend(RightColor, true, "marked and ran");
                    Legend(MissedColor, true, "marked, did not run");
                    Legend(UnexpectedColor, false, "ran, not marked");
                    GUILayout.FlexibleSpace();
                }
                if (revealFile.Length > 0) GUILayout.Label("Saved in " + revealFile, EditorStyles.miniLabel);
            }
        }

        void Legend(Color c, bool filled, string text)
        {
            Rect r = GUILayoutUtility.GetRect(12f, 12f, 16f, 16f, GUILayout.Width(12f));
            var square = new Rect(r.x + 2f, r.y + 4f, 8f, 8f);
            if (filled) EditorGUI.DrawRect(square, c);
            else DrawOutline(square, c);
            GUILayout.Label(text, EditorStyles.miniLabel);
            GUILayout.Space(8f);
        }

        // ---------------------------------------------------------------- state

        void StartPredict()
        {
            predictOn = true;
            revealed = false;
            needBaseline = false;
            predictScript = ScriptPath(target);
            markedLines.Clear();
            expectedNames.Clear();
            expectedValues.Clear();
            carriedLines.Clear();
            carriedCounts.Clear();
            ClearReveal();
            TakeBaseline();
        }

        void StopPredict()
        {
            predictOn = false;
            revealed = false;
            markedLines.Clear();
            expectedNames.Clear();
            expectedValues.Clear();
            baseLines.Clear();
            baseCounts.Clear();
            carriedLines.Clear();
            carriedCounts.Clear();
            ClearReveal();
        }

        void ClearReveal()
        {
            revealLines.Clear();
            revealCounts.Clear();
            revealNames.Clear();
            revealValues.Clear();
            revealResult = "";
            revealFile = "";
        }

        // Called at the start of Load: another script means another prediction.
        void PredictOnLoad()
        {
            if (predictOn && target != null && ScriptPath(target) != predictScript) StopPredict();
        }

        // Called at the end of Load, when the methods are known again.
        void PredictAfterLoad()
        {
            if (predictOn && !revealed && needBaseline && methods.Length > 0)
            {
                needBaseline = false;
                TakeBaseline();
            }
        }

        void PredictOnPlayModeChanged(PlayModeStateChange change)
        {
            if (!predictOn || revealed) return;
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                // Keep what ran so far, and start the counters of the new Play session from zero.
                Checkpoint();
                if (Ready) Coverage.ResetAll();
                baseLines.Clear();
                baseCounts.Clear();
            }
            else if (change == PlayModeStateChange.ExitingPlayMode)
            {
                Checkpoint();
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                // The counters may or may not have restarted: start again from what they say now.
                if (methods.Length > 0) TakeBaseline();
                else needBaseline = true;
            }
        }

        void TakeBaseline()
        {
            baseLines.Clear();
            baseCounts.Clear();
            foreach (KeyValuePair<int, uint> pair in ReadCounts(null))
            {
                baseLines.Add(pair.Key);
                baseCounts.Add((int)Math.Min(pair.Value, int.MaxValue));
            }
        }

        // Adds what ran since the baseline to what was carried, then moves the baseline to now.
        void Checkpoint()
        {
            Dictionary<int, int> since = RunSinceBaseline();
            Dictionary<int, int> carried = ToDictionary(carriedLines, carriedCounts);
            foreach (KeyValuePair<int, int> pair in since)
            {
                int before;
                carried.TryGetValue(pair.Key, out before);
                carried[pair.Key] = before + pair.Value;
            }
            FromDictionary(carried, carriedLines, carriedCounts);
            TakeBaseline();
        }

        Dictionary<int, int> RunSinceBaseline()
        {
            var result = new Dictionary<int, int>();
            Dictionary<int, int> baseline = ToDictionary(baseLines, baseCounts);
            foreach (KeyValuePair<int, uint> pair in ReadCounts(null))
            {
                int now = (int)Math.Min(pair.Value, int.MaxValue);
                int before;
                baseline.TryGetValue(pair.Key, out before);
                if (now < before) before = 0;   // the counters restarted
                if (now - before > 0) result[pair.Key] = now - before;
            }
            return result;
        }

        // Every executable line that ran since Predict was switched on, with how many times.
        Dictionary<int, int> RunSincePredict()
        {
            Dictionary<int, int> total = ToDictionary(carriedLines, carriedCounts);
            foreach (KeyValuePair<int, int> pair in RunSinceBaseline())
            {
                int before;
                total.TryGetValue(pair.Key, out before);
                total[pair.Key] = before + pair.Value;
            }
            var result = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> pair in total)
                if (IsExecutable(pair.Key) && pair.Value > 0) result[pair.Key] = pair.Value;
            return result;
        }

        static Dictionary<int, int> ToDictionary(List<int> keys, List<int> values)
        {
            var d = new Dictionary<int, int>();
            for (int i = 0; i < keys.Count && i < values.Count; i++) d[keys[i]] = values[i];
            return d;
        }

        static void FromDictionary(Dictionary<int, int> d, List<int> keys, List<int> values)
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<int, int> pair in d)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        bool IsMarked(int line)
        {
            return markedLines.Contains(line);
        }

        void ToggleMarked(int line)
        {
            if (!markedLines.Remove(line)) markedLines.Add(line);
        }

        string ExpectedFor(string field)
        {
            int i = expectedNames.IndexOf(field);
            return i >= 0 && i < expectedValues.Count ? expectedValues[i] : "";
        }

        void SetExpected(string field, string value)
        {
            int i = expectedNames.IndexOf(field);
            if (i < 0)
            {
                expectedNames.Add(field);
                expectedValues.Add(value);
            }
            else if (i < expectedValues.Count)
            {
                expectedValues[i] = value;
            }
        }

        int RevealCount(int line)
        {
            int i = revealLines.IndexOf(line);
            return i >= 0 && i < revealCounts.Count ? revealCounts[i] : 0;
        }

        string RevealActualFor(string field)
        {
            int i = revealNames.IndexOf(field);
            return i >= 0 && i < revealValues.Count ? revealValues[i] : "";
        }

        // ---------------------------------------------------------------- reveal

        void Reveal()
        {
            ReadValues(false);
            Dictionary<int, int> ran = RunSincePredict();
            FromDictionary(ran, revealLines, revealCounts);

            var names = new List<string>();
            var expected = new List<string>();
            var actual = new List<string>();
            revealNames.Clear();
            revealValues.Clear();
            foreach (System.Reflection.FieldInfo f in fields)
            {
                string value;
                valueByField.TryGetValue(f.Name, out value);
                revealNames.Add(f.Name);
                revealValues.Add(value == null ? "" : value);
                string typed = ExpectedFor(f.Name).Trim();
                if (typed.Length == 0) continue;
                names.Add(f.Name);
                expected.Add(typed);
                actual.Add(value == null ? "" : value);
            }

            PredictCheck.Result result = PredictCheck.Compare(markedLines, ran.Keys, expected, actual);
            revealResult = PredictCheck.Summary(result);
            revealed = true;

            string scriptName = target != null ? target.GetType().Name : Path.GetFileNameWithoutExtension(predictScript);
            string record = PredictCheck.Record(scriptName, predictScript, DateTime.Now, lines, result, ran, names, expected, actual);
            revealFile = SaveRecord(scriptName, record);
        }

        static string SaveRecord(string scriptName, string record)
        {
            try
            {
                Directory.CreateDirectory(PredictionsFolder);
                string path = PredictionsFolder + "/CodeWatch-" + scriptName + ".md";
                if (!File.Exists(path)) File.WriteAllText(path, "# Predictions: CodeWatch-" + scriptName + "\n\n");
                File.AppendAllText(path, record);
                AssetDatabase.ImportAsset(path);
                return path;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Code Watch could not save the prediction: " + ex.Message);
                return "";
            }
        }
    }

    // The comparison and the record of a prediction, with no editor state, so it can be tested alone.
    public static class PredictCheck
    {
        public class Result
        {
            public List<int> markedAndRan = new List<int>();
            public List<int> markedNotRan = new List<int>();
            public List<int> ranNotMarked = new List<int>();
            public List<bool> valueRight = new List<bool>();
            public int LinesRight { get { return markedAndRan.Count; } }
            public int LinesTotal { get { return markedAndRan.Count + markedNotRan.Count + ranNotMarked.Count; } }
            public int ValuesRight
            {
                get
                {
                    int n = 0;
                    foreach (bool b in valueRight) if (b) n++;
                    return n;
                }
            }
        }

        // A line is right when it was marked and it ran. The lines counted are the ones that were
        // marked or ran (or both). Values are compared one by one.
        public static Result Compare(IEnumerable<int> marked, IEnumerable<int> ran, IList<string> expected, IList<string> actual)
        {
            var result = new Result();
            var ranSet = new HashSet<int>(ran);
            var markedSet = new HashSet<int>(marked);
            foreach (int line in markedSet)
            {
                if (ranSet.Contains(line)) result.markedAndRan.Add(line);
                else result.markedNotRan.Add(line);
            }
            foreach (int line in ranSet)
                if (!markedSet.Contains(line)) result.ranNotMarked.Add(line);
            result.markedAndRan.Sort();
            result.markedNotRan.Sort();
            result.ranNotMarked.Sort();
            if (expected != null && actual != null)
                for (int i = 0; i < expected.Count && i < actual.Count; i++)
                    result.valueRight.Add(CodeLines.ValueMatches(expected[i], actual[i]));
            return result;
        }

        public static string Summary(Result r)
        {
            string text;
            if (r.LinesTotal == 0) text = "No line was marked and no line ran.";
            else text = r.LinesRight + " of " + r.LinesTotal + (r.LinesTotal == 1 ? " line right." : " lines right.");
            if (r.valueRight.Count > 0)
                text += " " + r.ValuesRight + " of " + r.valueRight.Count + (r.valueRight.Count == 1 ? " value right." : " values right.");
            return text;
        }

        public static string Record(string scriptName, string scriptPath, DateTime when, string[] lines, Result r,
            IDictionary<int, int> ranCounts, IList<string> names, IList<string> expected, IList<string> actual)
        {
            var sb = new StringBuilder();
            sb.Append("## ").Append(when.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(" · ").Append(scriptName).Append('\n');
            if (!string.IsNullOrEmpty(scriptPath)) sb.Append("Script: ").Append(scriptPath).Append('\n');
            sb.Append("Result: ").Append(Summary(r)).Append("\n\n");

            var marked = new List<int>(r.markedAndRan);
            marked.AddRange(r.markedNotRan);
            marked.Sort();
            var ran = new List<int>(r.markedAndRan);
            ran.AddRange(r.ranNotMarked);
            ran.Sort();
            sb.Append("Lines marked: ").Append(marked.Count == 0 ? "none" : Join(marked)).Append('\n');
            sb.Append("Lines that ran: ").Append(ran.Count == 0 ? "none" : Join(ran)).Append("\n\n");

            var all = new List<int>(marked);
            foreach (int line in r.ranNotMarked) all.Add(line);
            all.Sort();
            foreach (int line in all)
            {
                string code = lines != null && line >= 1 && line <= lines.Length ? lines[line - 1].Trim() : "";
                int count = 0;
                if (ranCounts != null) ranCounts.TryGetValue(line, out count);
                string verdict;
                if (r.markedAndRan.Contains(line)) verdict = "marked, ran ×" + count;
                else if (r.markedNotRan.Contains(line)) verdict = "marked, did not run";
                else verdict = "ran ×" + count + ", not marked";
                sb.Append("- ").Append(line).Append(" `").Append(code.Replace("`", "'")).Append("` ").Append(verdict).Append('\n');
            }

            if (names != null && names.Count > 0)
            {
                sb.Append("\nValues:\n");
                for (int i = 0; i < names.Count && i < expected.Count && i < actual.Count; i++)
                {
                    bool right = i < r.valueRight.Count && r.valueRight[i];
                    sb.Append("- ").Append(names[i]).Append(": expected ").Append(expected[i]).Append(", was ").Append(actual[i])
                      .Append(right ? " (right)" : " (not right)").Append('\n');
                }
            }
            sb.Append('\n');
            return sb.ToString();
        }

        static string Join(List<int> numbers)
        {
            var parts = new string[numbers.Count];
            for (int i = 0; i < numbers.Count; i++) parts[i] = numbers[i].ToString(CultureInfo.InvariantCulture);
            return string.Join(", ", parts);
        }
    }
}
