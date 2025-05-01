#pragma warning disable 0618

using UnityEditor;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [InitializeOnLoad]
    public class MISMotorcycle
    {
        // ----------------------------------------------------------------------------------------------------
        // Package
        public static string PACKAGE_VERSION = "1.3.10";
        public static int PACKAGE_VERSION_CODE = 19;


        // ----------------------------------------------------------------------------------------------------
        // MIS
        public static string MIS_MIN_VERSION = "2.7.9";
        public static int MIN_MIS_VERSION_CODE = 60;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        static MISMotorcycle()
        {
            if (!SessionState.GetBool(MISFeature.MIS_PACKAGE_MOTORCYCLE, false))
            {
                if (!HasValidVersion())
                    Debug.LogError("Currently installed MIS version is not compatible with " + MISFeature.MIS_PACKAGE_MOTORCYCLE + ". Please upgrade MIS to make it work properly.");

                SessionState.SetBool(MISFeature.MIS_PACKAGE_MOTORCYCLE, true);
            }

            if (MISMainSetup.HasMISRefactoringDone && !ScriptingDefineSymbolManager.IsSymbolAlreadyDefined(MISFeature.MIS_FEATURE_MOTORCYCLE))
            {
                ScriptingDefineSymbolManager.AddDefineSymbol(MISFeature.MIS_FEATURE_MOTORCYCLE);
                MISMainSetup.SetAddonVersion(MISFeature.MIS_MOTORCYCLE_OPTION_PATH, PACKAGE_VERSION);

                // Vehicle Layer
                if (!MISEditorTagLayer.HasUnityLayer(MISEditorTagLayer.LAYER_VEHICLE))
                {
                    MISEditorTagLayer.AddUnityLayer(MISEditorTagLayer.LAYER_VEHICLE, 30);

                    Debug.LogWarningFormat("[{0}] Vehicle layer has been added on the 30th layer mask index.", MISFeature.MIS_PACKAGE_MOTORCYCLE);
                }
            }
            else if (!MISMainSetup.HasMISRefactoringDone && ScriptingDefineSymbolManager.IsSymbolAlreadyDefined(MISFeature.MIS_FEATURE_MOTORCYCLE))
            {
                ScriptingDefineSymbolManager.RemoveDefineSymbol(MISFeature.MIS_FEATURE_MOTORCYCLE);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public static bool HasValidVersion()
        {
            return MIN_MIS_VERSION_CODE <= MIS.MIS_VERSION_CODE;
        }
    }
}
