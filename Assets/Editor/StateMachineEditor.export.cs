﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using JinkeGroup.Logic.StateMachineInternal;

namespace JinkeGroup.Logic {

    public abstract partial class StateMachineEditor {

        protected void OnExportGUI() {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Export path", TargetStateMachine.ExportPath);
            if (GUILayout.Button("...", GUILayout.Width(30.0f))) {
                TargetStateMachine.ExportPath = EditorUtility.SaveFilePanelInProject("State Machine export path", this.name + ".cs", "cs", "Please select path where to export StateMachine data");
            }
            EditorGUILayout.EndHorizontal();
        }

        protected string FixName(string name) {
            if (string.IsNullOrEmpty(name)) {
                return name;
            }
            return name.Trim(' ').Replace(" ", "");
        }

        protected void Export() {
            if (string.IsNullOrEmpty(TargetStateMachine.ExportPath)) {
                EditorUtility.DisplayDialog("Error", "Export path not set!", "OK");
            } else {
                StreamWriter sw = File.CreateText(TargetStateMachine.ExportPath);

                sw.WriteLine("// AUTOGENERATED FILE, DO NOT MODIFY, M'KAY?\n\n");
                sw.WriteLine("namespace JinkeGroup.Logic {");

                sw.WriteLine("   public static class " + TargetStateMachine.name + " {");

                sw.WriteLine("      public static class Layers {");
                for (int a = 0; a < TargetStateMachine.Layers.Length; a++) {
                    string name = FixName(TargetStateMachine.Layers[a].Name);
                    int val = a;
                    if (string.IsNullOrEmpty(name)) {
                        continue;
                    }
                    sw.WriteLine("         public const int " + name + " = " + val + ";");
                }
                sw.WriteLine("      }");
                sw.WriteLine("");

                sw.WriteLine("      public static class Attributes {");
                for (int a = 0; a < TargetStateMachine.AttributeNames.Length; a++) {
                    string attrName = FixName(TargetStateMachine.AttributeNames[a]);
                    int val = 1 << a;
                    if (string.IsNullOrEmpty(attrName)) {
                        continue;
                    }
                    sw.WriteLine("         public const int " + attrName + " = " + val + ";");
                }
                sw.WriteLine("      }");
                sw.WriteLine("");

                sw.WriteLine("      public static class Parameters {");
                for (int a = 0; a < TargetStateMachine.Parameters.Length; a++) {
                    Parameter p = TargetStateMachine.Parameters[a];
                    string s = FixName(p.Name);
                    sw.WriteLine("         public const int " + s + " = " + a + ";");
                }
                sw.WriteLine("      }");
                sw.WriteLine("");


                sw.WriteLine("      public static class Enum {");
                for (int a = 0; a < TargetStateMachine.Enums.Length; a++) {
                    Enum e = TargetStateMachine.Enums[a];
                    sw.WriteLine("         public static class " + FixName(e.Name) + " {");
                    for (int b = 0; b < e.Strings.Length; b++) {
                        sw.WriteLine("            public const int " + FixName(e.Strings[b]) + " = " + b + ";");
                    }
                    sw.WriteLine("         }");
                    sw.WriteLine("");
                    sw.WriteLine("         public enum " + FixName(e.Name) + "Enum {");
                    for (int b = 0; b < e.Strings.Length; b++) {
                        sw.WriteLine("            " + FixName(e.Strings[b]) + " = " + b + ",");
                    }
                    sw.WriteLine("         }");
                    sw.WriteLine("");
                }

                sw.WriteLine("      }");

                sw.WriteLine("   }");

                sw.WriteLine("}");
                sw.Close();
            }
        }
    }
}