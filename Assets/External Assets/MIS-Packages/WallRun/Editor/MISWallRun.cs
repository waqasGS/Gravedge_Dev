#pragma warning disable 0618

using UnityEditor;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [InitializeOnLoad]
    public class MISWallRun
    {
        // ----------------------------------------------------------------------------------------------------
        // Package
        public static string PACKAGE_VERSION = "1.1.11";
        public static int PACKAGE_VERSION_CODE = 16;


        // ----------------------------------------------------------------------------------------------------
        // MIS
        public static string MIS_MIN_VERSION = "2.7.14";
        public static int MIN_MIS_VERSION_CODE = 65;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        static MISWallRun()
        {
            if (!SessionState.GetBool(MISFeature.MIS_PACKAGE_WALLRUN, false))
            {
                if (!HasValidVersion())
                    Debug.LogError("Currently installed MIS version is not compatible with " + MISFeature.MIS_PACKAGE_WALLRUN + ". Please upgrade MIS to make it work properly.");

                SessionState.SetBool(MISFeature.MIS_PACKAGE_WALLRUN, true);
            }

            if (MISMainSetup.HasMISRefactoringDone && !ScriptingDefineSymbolManager.IsSymbolAlreadyDefined(MISFeature.MIS_FEATURE_WALLRUN))
            {
                ScriptingDefineSymbolManager.AddDefineSymbol(MISFeature.MIS_FEATURE_WALLRUN);
                MISMainSetup.SetAddonVersion(MISFeature.MIS_WALLRUN_OPTION_PATH, PACKAGE_VERSION);

                // WallRun Tag
                if (!MISEditorTagLayer.HasUnityTag(MISEditorTagLayer.TAG_WALLRUN))
                {
                    MISEditorTagLayer.AddUnityTag(MISEditorTagLayer.TAG_WALLRUN);

                    Debug.LogWarning("[MIS-WallRun] WallRun tag has been added.");
                }
            }
            else if (!MISMainSetup.HasMISRefactoringDone && ScriptingDefineSymbolManager.IsSymbolAlreadyDefined(MISFeature.MIS_FEATURE_WALLRUN))
            {
                ScriptingDefineSymbolManager.RemoveDefineSymbol(MISFeature.MIS_FEATURE_WALLRUN);
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

