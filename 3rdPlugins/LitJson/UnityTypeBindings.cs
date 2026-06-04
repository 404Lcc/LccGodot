using System;
using System.Collections;
using Godot;
using LitJson.Extensions;

namespace LitJson
{

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    /// <summary>
    /// Unity内建类型拓展
    /// </summary>
    public static class UnityTypeBindings
    {

        static bool registerd;

        static UnityTypeBindings()
        {
            Register();
        }

        public static void Register()
        {

            if (registerd) return;
            registerd = true;


            // 注册Type类型的Exporter
            JsonMapper.RegisterExporter<Type>((v, w) =>
            {
                w.Write(v.FullName);
            });

            JsonMapper.RegisterImporter<string, Type>((s) =>
            {
                return Type.GetType(s);
            });

            // 注册Vector2类型的Exporter
            Action<Vector2, JsonWriter> writeVector2 = (v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.X);
                w.WriteProperty("y", v.Y);
                w.WriteObjectEnd();
            };

            JsonMapper.RegisterExporter<Vector2>((v, w) =>
            {
                writeVector2(v, w);
            });

            // 注册Vector3类型的Exporter
            Action<Vector3, JsonWriter> writeVector3 = (v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.X);
                w.WriteProperty("y", v.Y);
                w.WriteProperty("z", v.Z);
                w.WriteObjectEnd();
            };

            JsonMapper.RegisterExporter<Vector3>((v, w) =>
            {
                writeVector3(v, w);
            });

            // 注册Vector4类型的Exporter
            JsonMapper.RegisterExporter<Vector4>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.X);
                w.WriteProperty("y", v.Y);
                w.WriteProperty("z", v.Z);
                w.WriteProperty("w", v.W);
                w.WriteObjectEnd();
            });

            // 注册Quaternion类型的Exporter
            JsonMapper.RegisterExporter<Quaternion>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.X);
                w.WriteProperty("y", v.Y);
                w.WriteProperty("z", v.Z);
                w.WriteProperty("w", v.W);
                w.WriteObjectEnd();
            });

            // 注册Color类型的Exporter
            JsonMapper.RegisterExporter<Color>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("r", v.R);
                w.WriteProperty("g", v.G);
                w.WriteProperty("b", v.B);
                w.WriteProperty("a", v.A);
                w.WriteObjectEnd();
            });
        }

    }
}
