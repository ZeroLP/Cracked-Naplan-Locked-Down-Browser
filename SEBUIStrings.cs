
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SebWindowsClient
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  public class SEBUIStrings
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal SEBUIStrings()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static ResourceManager ResourceManager
    {
      get
      {
        if (SEBUIStrings.resourceMan == null)
          SEBUIStrings.resourceMan = new ResourceManager("SebWindowsClient.SEBUIStrings", typeof (SEBUIStrings).Assembly);
        return SEBUIStrings.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static CultureInfo Culture
    {
      get
      {
        return SEBUIStrings.resourceCulture;
      }
      set
      {
        SEBUIStrings.resourceCulture = value;
      }
    }

    public static string alertWebSocketPortBlocked
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (alertWebSocketPortBlocked), SEBUIStrings.resourceCulture);
      }
    }

    public static string alertWebSocketPortBlockedMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (alertWebSocketPortBlockedMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string cannotOpenSEBLink
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (cannotOpenSEBLink), SEBUIStrings.resourceCulture);
      }
    }

    public static string cannotOpenSEBLinkMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (cannotOpenSEBLinkMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string certificateDecryptingError
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (certificateDecryptingError), SEBUIStrings.resourceCulture);
      }
    }

    public static string certificateNotFoundInStore
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (certificateNotFoundInStore), SEBUIStrings.resourceCulture);
      }
    }

    public static string ChooseEmbeddedCert
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (ChooseEmbeddedCert), SEBUIStrings.resourceCulture);
      }
    }

    public static string closeProcesses
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (closeProcesses), SEBUIStrings.resourceCulture);
      }
    }

    public static string closeProcessesQuestion
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (closeProcessesQuestion), SEBUIStrings.resourceCulture);
      }
    }

    public static string confirmQuitting
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (confirmQuitting), SEBUIStrings.resourceCulture);
      }
    }

    public static string confirmQuittingQuestion
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (confirmQuittingQuestion), SEBUIStrings.resourceCulture);
      }
    }

    public static string createNewDesktopFailed
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (createNewDesktopFailed), SEBUIStrings.resourceCulture);
      }
    }

    public static string createNewDesktopFailedReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (createNewDesktopFailedReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string decryptingSettingsFailed
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (decryptingSettingsFailed), SEBUIStrings.resourceCulture);
      }
    }

    public static string decryptingSettingsFailedReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (decryptingSettingsFailedReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string detectedVirtualMachine
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (detectedVirtualMachine), SEBUIStrings.resourceCulture);
      }
    }

    public static string detectedVirtualMachineForbiddenMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (detectedVirtualMachineForbiddenMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string enterAdminPasswordRequired
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (enterAdminPasswordRequired), SEBUIStrings.resourceCulture);
      }
    }

    public static string enterAdminPasswordRequiredAgain
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (enterAdminPasswordRequiredAgain), SEBUIStrings.resourceCulture);
      }
    }

    public static string enterCurrentAdminPwdForReconfiguring
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (enterCurrentAdminPwdForReconfiguring), SEBUIStrings.resourceCulture);
      }
    }

    public static string enterCurrentAdminPwdForReconfiguringAgain
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (enterCurrentAdminPwdForReconfiguringAgain), SEBUIStrings.resourceCulture);
      }
    }

    public static string enterEncryptionPassword
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (enterEncryptionPassword), SEBUIStrings.resourceCulture);
      }
    }

    public static string enterEncryptionPasswordAgain
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (enterEncryptionPasswordAgain), SEBUIStrings.resourceCulture);
      }
    }

    public static string enterPassword
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (enterPassword), SEBUIStrings.resourceCulture);
      }
    }

    public static string enterPasswordAgain
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (enterPasswordAgain), SEBUIStrings.resourceCulture);
      }
    }

    public static string ErrorCaption
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (ErrorCaption), SEBUIStrings.resourceCulture);
      }
    }

    public static string errorDecryptingSettings
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (errorDecryptingSettings), SEBUIStrings.resourceCulture);
      }
    }

    public static string ErrorWhenOpeningSettingsFile
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (ErrorWhenOpeningSettingsFile), SEBUIStrings.resourceCulture);
      }
    }

    public static string forceSebServiceMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (forceSebServiceMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string identityExportError
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (identityExportError), SEBUIStrings.resourceCulture);
      }
    }

    public static string identityExportErrorMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (identityExportErrorMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string indicateMissingService
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (indicateMissingService), SEBUIStrings.resourceCulture);
      }
    }

    public static string indicateMissingServiceReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (indicateMissingServiceReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string integrityCheckError
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (integrityCheckError), SEBUIStrings.resourceCulture);
      }
    }

    public static string integrityCheckErrorReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (integrityCheckErrorReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string KeyboardLayout_CURRENTCULTURE
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (KeyboardLayout_CURRENTCULTURE), SEBUIStrings.resourceCulture);
      }
    }

    public static string loadingSettings
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (loadingSettings), SEBUIStrings.resourceCulture);
      }
    }

    public static string loadingSettingsFailed
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (loadingSettingsFailed), SEBUIStrings.resourceCulture);
      }
    }

    public static string loadingSettingsFailedReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (loadingSettingsFailedReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string loadingSettingsFailedWrongAdminPwd
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (loadingSettingsFailedWrongAdminPwd), SEBUIStrings.resourceCulture);
      }
    }

    public static string loadingSettingsNotAllowed
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (loadingSettingsNotAllowed), SEBUIStrings.resourceCulture);
      }
    }

    public static string loadingSettingsNotAllowedReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (loadingSettingsNotAllowedReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string locatePermittedApplication
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (locatePermittedApplication), SEBUIStrings.resourceCulture);
      }
    }

    public static string noEncryptionChosen
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (noEncryptionChosen), SEBUIStrings.resourceCulture);
      }
    }

    public static string noEncryptionChosenSaveUnencrypted
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (noEncryptionChosenSaveUnencrypted), SEBUIStrings.resourceCulture);
      }
    }

    public static string NoWLANInterface
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (NoWLANInterface), SEBUIStrings.resourceCulture);
      }
    }

    public static string openingSettingsFailed
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (openingSettingsFailed), SEBUIStrings.resourceCulture);
      }
    }

    public static string openingSettingsFailedMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (openingSettingsFailedMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string OSNotSupported
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (OSNotSupported), SEBUIStrings.resourceCulture);
      }
    }

    public static string passwordAdmin
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (passwordAdmin), SEBUIStrings.resourceCulture);
      }
    }

    public static string passwordQuit
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (passwordQuit), SEBUIStrings.resourceCulture);
      }
    }

    public static string passwordSettings
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (passwordSettings), SEBUIStrings.resourceCulture);
      }
    }

    public static string permittedApplicationNotFound
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (permittedApplicationNotFound), SEBUIStrings.resourceCulture);
      }
    }

    public static string permittedApplicationNotFoundMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (permittedApplicationNotFoundMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string quittingFailed
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (quittingFailed), SEBUIStrings.resourceCulture);
      }
    }

    public static string quittingFailedReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (quittingFailedReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string reconfiguringLocalSettings
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (reconfiguringLocalSettings), SEBUIStrings.resourceCulture);
      }
    }

    public static string reconfiguringLocalSettingsFailed
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (reconfiguringLocalSettingsFailed), SEBUIStrings.resourceCulture);
      }
    }

    public static string reconfiguringLocalSettingsFailedWrongAdminPwd
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (reconfiguringLocalSettingsFailedWrongAdminPwd), SEBUIStrings.resourceCulture);
      }
    }

    public static string reconfiguringLocalSettingsFailedWrongCurrentAdminPwd
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (reconfiguringLocalSettingsFailedWrongCurrentAdminPwd), SEBUIStrings.resourceCulture);
      }
    }

    public static string reconfiguringLocalSettingsFailedWrongPassword
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (reconfiguringLocalSettingsFailedWrongPassword), SEBUIStrings.resourceCulture);
      }
    }

    public static string reloadPage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (reloadPage), SEBUIStrings.resourceCulture);
      }
    }

    public static string reloadPageMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (reloadPageMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string restartExamConfirm
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (restartExamConfirm), SEBUIStrings.resourceCulture);
      }
    }

    public static string restartExamDefaultTitle
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (restartExamDefaultTitle), SEBUIStrings.resourceCulture);
      }
    }

    public static string restartExamEnterPassword
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (restartExamEnterPassword), SEBUIStrings.resourceCulture);
      }
    }

    public static string savingSettingsFailed
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (savingSettingsFailed), SEBUIStrings.resourceCulture);
      }
    }

    public static string savingSettingsFailedMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (savingSettingsFailedMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string savingSettingsSucceeded
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (savingSettingsSucceeded), SEBUIStrings.resourceCulture);
      }
    }

    public static string savingSettingsSucceededMessageConfigureClient
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (savingSettingsSucceededMessageConfigureClient), SEBUIStrings.resourceCulture);
      }
    }

    public static string savingSettingsSucceededStartExam
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (savingSettingsSucceededStartExam), SEBUIStrings.resourceCulture);
      }
    }

    public static string sebFileTypeName
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (sebFileTypeName), SEBUIStrings.resourceCulture);
      }
    }

    public static string sebReconfigured
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (sebReconfigured), SEBUIStrings.resourceCulture);
      }
    }

    public static string sebReconfiguredQuestion
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (sebReconfiguredQuestion), SEBUIStrings.resourceCulture);
      }
    }

    public static string sebReconfiguredRestartNeeded
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (sebReconfiguredRestartNeeded), SEBUIStrings.resourceCulture);
      }
    }

    public static string sebReconfiguredRestartNeededReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (sebReconfiguredRestartNeededReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsDuplicateSuffix
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsDuplicateSuffix), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsNotUsable
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsNotUsable), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsNotUsableReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsNotUsableReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsRequireNewDesktop
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsRequireNewDesktop), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsRequireNewDesktopReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsRequireNewDesktopReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsRequireNotNewDesktop
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsRequireNotNewDesktop), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsRequireNotNewDesktopReason
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsRequireNotNewDesktopReason), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsTitleDefaultSettings
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsTitleDefaultSettings), SEBUIStrings.resourceCulture);
      }
    }

    public static string settingsUntitledFilename
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (settingsUntitledFilename), SEBUIStrings.resourceCulture);
      }
    }

    public static string toolTipConnectedToWiFiNetwork
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (toolTipConnectedToWiFiNetwork), SEBUIStrings.resourceCulture);
      }
    }

    public static string toolTipNotConnectedToWiFiNetwork
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (toolTipNotConnectedToWiFiNetwork), SEBUIStrings.resourceCulture);
      }
    }

    public static string toolTipNoWiFiInterface
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (toolTipNoWiFiInterface), SEBUIStrings.resourceCulture);
      }
    }

    public static string toolTipOnScreenKeyboard
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (toolTipOnScreenKeyboard), SEBUIStrings.resourceCulture);
      }
    }

    public static string unconfirmedPasswordMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (unconfirmedPasswordMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string unconfirmedPasswordTitle
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (unconfirmedPasswordTitle), SEBUIStrings.resourceCulture);
      }
    }

    public static string unsavedChangesQuestion
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (unsavedChangesQuestion), SEBUIStrings.resourceCulture);
      }
    }

    public static string unsavedChangesTitle
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (unsavedChangesTitle), SEBUIStrings.resourceCulture);
      }
    }

    public static string webSocketServerNotStarted
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (webSocketServerNotStarted), SEBUIStrings.resourceCulture);
      }
    }

    public static string webSocketServerNotStartedMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (webSocketServerNotStartedMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string WlanConnecting
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (WlanConnecting), SEBUIStrings.resourceCulture);
      }
    }

    public static string WlanConnectionFailedMessage
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (WlanConnectionFailedMessage), SEBUIStrings.resourceCulture);
      }
    }

    public static string WlanConnectionFailedMessageTitle
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (WlanConnectionFailedMessageTitle), SEBUIStrings.resourceCulture);
      }
    }

    public static string WlanNoNetworkInterfaceFound
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (WlanNoNetworkInterfaceFound), SEBUIStrings.resourceCulture);
      }
    }

    public static string WlanNoNetworksFound
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (WlanNoNetworksFound), SEBUIStrings.resourceCulture);
      }
    }

    public static string WLANNotConnected
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (WLANNotConnected), SEBUIStrings.resourceCulture);
      }
    }

    public static string WlanThatYouHaveUsedBefore
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (WlanThatYouHaveUsedBefore), SEBUIStrings.resourceCulture);
      }
    }

    public static string WlanYouCanOnlyConnectToNetworks
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (WlanYouCanOnlyConnectToNetworks), SEBUIStrings.resourceCulture);
      }
    }

    public static string wrongQuitRestartPasswordText
    {
      get
      {
        return SEBUIStrings.ResourceManager.GetString(nameof (wrongQuitRestartPasswordText), SEBUIStrings.resourceCulture);
      }
    }
  }
}
