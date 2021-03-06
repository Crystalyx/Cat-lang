using System.Collections.Generic;
using System.IO;
using Cat.Structure;
using Cat.Utilities;

namespace Cat.AbstractStructure
{
    public static class CatClassLoader
    {

        public static string BasePath = "classes/";
        /// <summary>
        /// Loads class internal fields to use them in program to create objects
        /// </summary>
        /// <param name="className"> Name of class to load</param>
        /// <returns></returns>
        public static (CatClass clazz,int index) LoadClassFile(string fileName)
        {
            var lines = File.ReadAllLines(BasePath+fileName);

            var search = false;
            var className = "";
            (int i, int j) index = (0, 0);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var words = line.Split();
                for (var j = 0; j < words.Length; j++)
                {
                    if (words[j] == "#")
                    {
                        j = words.Length;
                        continue;
                    }

                    //Console.WriteLine(words[j]);
                    if (!search)
                    {
                        if (words[j] == "class")
                        {
                            //Console.WriteLine("class found");
                            search = true;
                        }
                    }
                    else
                    {
                        className = words[j];
                        index = (i, j);
                        goto search;
                    }
                }
            }

            search:
            
            if (className != "")
            {
                var bracesNesting = 0;

                var classProperties = new List<CatProperty>();

                int modifiers = 0;
                for (var i = index.i; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var firstTime = true;
                    var words = line.Split();
                    for (var j = index.j; j < words.Length; j++)
                    {
                        if (words[j] == "#")
                            continue;
                        if (words[j] == ";")
                        {
                            modifiers = 0;
                        }

                        if (words[j - 1] == ":") //Static method : methodName ( int a , string b ) ~ returnType
                        {
                            if (ModifierHandler.IsMethod(modifiers)) //Method
                            {

                                int k = j;
                                string signature = "";
                                while (k < words.Length && words[k] != ";")
                                {
                                    signature += words[k] + " ";
                                    k++;
                                }

                                signature = signature.Trim();
                                string link = fileName + ":" + i; //i-th line of file:className.cls
                                string[] sign = signature.Split('~');
                                var rawMethod = new CatMethod(sign[0].Trim(), sign[1].Trim(), className.Trim(), i) {Modifiers = modifiers};
                                classProperties.Add(rawMethod);
                                
                                j = k;
                                modifiers = 0;
                                continue;
                            }
                            if (ModifierHandler.IsConstructor(modifiers)) //Method
                            {

                                int k = j;
                                string signature = "";
                                while (k < words.Length && words[k] != ";")
                                {
                                    signature += words[k] + " ";
                                    k++;
                                }

                                signature = signature.Trim();
                                string link = fileName + ":" + i; //i-th line of file:className.cls
                                var rawMethod = new CatConstructor(signature,"", className.Trim(), i) {Modifiers = modifiers};
                                classProperties.Add(rawMethod);
                                
                                j = k;
                                modifiers = 0;
                                continue;
                            }
                            if (ModifierHandler.IsField(modifiers)) // field
                            {

                                var name = words[j];
                                var type = words[j + 2];

                                //words[i+3] == ":"
                                //words[i+4] == "="
                                //words[i+5] == "2"//value
                                var k = j + 4;
                                var expr = "";
                                while (k < words.Length && words[k] != ";")
                                {
                                    expr += words[k] + " ";
                                    k++;
                                }

                                if (expr == "")
                                    expr = CatCore.V0;
                                var rawField = new CatField(name.Trim(), type.Trim(),null) {Modifiers = modifiers};//todo null
                                classProperties.Add(rawField);
                                j += 4;
                                modifiers = 0;
                                continue;
                            }
                        }

                        if (words[j] == "static")
                        {
                            modifiers += (int)Modifier.Static;
                        }

                        if (words[j] == "constructor")
                        {
                            modifiers += (int)Modifier.Constructor;
                        }

                        if (words[j] == "field")
                        {
                            modifiers += (int)Modifier.Field;
                        }

                        if (words[j] == "method")
                        {
                            modifiers += (int)Modifier.Method;
                        }

                        if (words[j] == "{")
                        {
                            bracesNesting++;
                            firstTime = false;
                            continue;
                        }

                        if (words[j] == "}")
                        {
                            bracesNesting--;
                            continue;
                        }

                        if (bracesNesting <= 0 && !firstTime)
                        {
                            goto after;
                        }
                    }
                }

                after: //All fields and methods are read

                var clazz = new CatClass(className, classProperties.ToArray());

                //var size = 5 + nonStaticFields.Count * 2 + staticFields.Count * 3 + nonStaticMethods.Count + staticMethods.Count * 2;

                var ret = HeapHandler.LoadObjectToHeap(clazz);

                CatCore.Types.Add(className, ret);

                foreach (var prop in clazz.Properties)
                {
                    if (prop is CatConstructor constr)
                    {
                        constr.ReturnType = ret;
                    }
                }
                
                return (clazz,ret);
            }

            return (null,CatCore.L0);
        }

        /// <summary>
        /// Method that will remove all data about provided class
        /// </summary>
        /// <param name="className"> Class to unload</param>
        public static void UnloadClass(string className)
        {
            CatCore.Types.Remove(className);
            CatCore.Classes.Remove(className);
        }
    }
}