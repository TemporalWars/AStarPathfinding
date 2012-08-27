// *****************************************************
// Using AStar Sample, created in C#
// By Ben Scharbach
// Image-Nexus, LLC. (4/16/2012)
// *****************************************************
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace UsingAStarSample.ImageNexus_LateBinder
{
    /// <summary>
    /// The <see cref="LateBinder"/> class is used to load and late bind some Assembly (dll) file.
    /// </summary>
    public static class LateBinder
    {
        /// <summary>
        /// Allows LateBinding some Assembly (dll) file, and then will
        /// instantiate the given 'ClassName', and return the object to the caller.
        /// </summary>
        /// <param name="assemblyFile">AssemblyFile name to load</param>
        /// <param name="className">Class Name to instantiate within Assembly</param>
        /// <param name="instantiatedObject">(OUT) Instantiated object</param>
        /// <returns>True/False of success</returns>
        public static bool LateBindAssembly(string assemblyFile, string className, out object instantiatedObject)
        {
            instantiatedObject = null;

            try
            {
                var assemblyToLoad = Assembly.LoadFrom("0LateBinds/" + assemblyFile);
                var mytypes = assemblyToLoad.GetTypes();

                // Search for Instance to instantiate from Assembly.
                foreach (var type in mytypes)
                {
                    // locate class instance to instantiate.
                    if (type.Name != className) continue;

                    instantiatedObject = Activator.CreateInstance(type);
                    return true;
                }

                // Name not found
                return false;
            }
            // Capture the possibility of the DLL not being in the folder at all.
            catch (FileNotFoundException) // PC throws this.
            {
                Console.WriteLine(@"DLL Component {0} not found.  Therefore, this will be skipped for late binding.", assemblyFile);
                return false;
            }
            catch (IOException) // XBOX throws this.
            {
                Console.WriteLine(@"DLL Component {0} not found.  Therefore, this will be skipped for late binding.", assemblyFile);
                return false;
            }
            catch (ReflectionTypeLoadException err)
            {
                if (err.LoaderExceptions != null)
                {
                    // List out each LoaderException error to console.
                    foreach (var loaderException in err.LoaderExceptions)
                    {
                        Console.WriteLine(@"LoaderExceptions reflection error - {0}", loaderException.Message);
                    }

                    MessageBox.Show(
                        @"Late-Binding failed, due to a Loading Exception on the Interface!  This usually occurs if you have an outdated interface; please update your interface for the assembly you are trying to late-bind.",
                        @"LateBind Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                Console.WriteLine(@"DLL Component {0} reflection error.  Therefore, this will be skipped for late binding.", assemblyFile);
                return false;
            }
        }
    }
}
