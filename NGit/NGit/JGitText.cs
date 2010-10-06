using NGit;
using NGit.Nls;
using Sharpen;

namespace NGit
{
	/// <summary>Translation bundle for JGit core</summary>
	public class JGitText : TranslationBundle
	{
		/// <returns>an instance of this translation bundle</returns>
		public static JGitText Get()
		{
			return NLS.GetBundleFor<JGitText>();
		}

		public string DIRCChecksumMismatch;

		public string DIRCExtensionIsTooLargeAt;

		public string DIRCExtensionNotSupportedByThisVersion;

		public string DIRCHasTooManyEntries;

		public string DIRCUnrecognizedExtendedFlags;

		public string JRELacksMD5Implementation;

		public string URINotSupported;

		public string URLNotFound;

		public string aNewObjectIdIsRequired;

		public string abbreviationLengthMustBeNonNegative;

		public string advertisementCameBefore;

		public string advertisementOfCameBefore;

		public string amazonS3ActionFailed;

		public string amazonS3ActionFailedGivingUp;

		public string ambiguousObjectAbbreviation;

		public string anExceptionOccurredWhileTryingToAddTheIdOfHEAD;

		public string anSSHSessionHasBeenAlreadyCreated;

		public string atLeastOnePathIsRequired;

		public string atLeastOnePatternIsRequired;

		public string atLeastTwoFiltersNeeded;

		public string badBase64InputCharacterAt;

		public string badEntryDelimiter;

		public string badEntryName;

		public string badEscape;

		public string badGroupHeader;

		public string badObjectType;

		public string badSectionEntry;

		public string base64InputNotProperlyPadded;

		public string baseLengthIncorrect;

		public string bareRepositoryNoWorkdirAndIndex;

		public string blobNotFound;

		public string blobNotFoundForPath;

		public string cannotBeCombined;

		public string cannotCombineTreeFilterWithRevFilter;

		public string cannotCommitOnARepoWithState;

		public string cannotCommitWriteTo;

		public string cannotConnectPipes;

		public string cannotConvertScriptToText;

		public string cannotCreateConfig;

		public string cannotCreateDirectory;

		public string cannotCreateHEAD;

		public string cannotDeleteFile;

		public string cannotDeleteStaleTrackingRef2;

		public string cannotDeleteStaleTrackingRef;

		public string cannotDetermineProxyFor;

		public string cannotDownload;

		public string cannotExecute;

		public string cannotGet;

		public string cannotListRefs;

		public string cannotLock;

		public string cannotLockFile;

		public string cannotLockPackIn;

		public string cannotMatchOnEmptyString;

		public string cannotMoveIndexTo;

		public string cannotMovePackTo;

		public string cannotOpenService;

		public string cannotParseGitURIish;

		public string cannotRead;

		public string cannotReadBlob;

		public string cannotReadCommit;

		public string cannotReadFile;

		public string cannotReadHEAD;

		public string cannotReadObject;

		public string cannotReadTree;

		public string cannotResolveLocalTrackingRefForUpdating;

		public string cannotStoreObjects;

		public string cannotUnloadAModifiedTree;

		public string cannotWorkWithOtherStagesThanZeroRightNow;

		public string cantFindObjectInReversePackIndexForTheSpecifiedOffset;

		public string cantPassMeATree;

		public string channelMustBeInRange0_255;

		public string characterClassIsNotSupported;

		public string checkoutConflictWithFile;

		public string checkoutConflictWithFiles;

		public string classCastNotA;

		public string collisionOn;

		public string commandWasCalledInTheWrongState;

		public string commitAlreadyExists;

		public string commitMessageNotSpecified;

		public string commitOnRepoWithoutHEADCurrentlyNotSupported;

		public string compressingObjects;

		public string connectionFailed;

		public string connectionTimeOut;

		public string contextMustBeNonNegative;

		public string corruptObjectBadStream;

		public string corruptObjectBadStreamCorruptHeader;

		public string corruptObjectGarbageAfterSize;

		public string corruptObjectIncorrectLength;

		public string corruptObjectInvalidEntryMode;

		public string corruptObjectInvalidMode2;

		public string corruptObjectInvalidMode3;

		public string corruptObjectInvalidMode;

		public string corruptObjectInvalidType2;

		public string corruptObjectInvalidType;

		public string corruptObjectMalformedHeader;

		public string corruptObjectNegativeSize;

		public string corruptObjectNoAuthor;

		public string corruptObjectNoCommitter;

		public string corruptObjectNoHeader;

		public string corruptObjectNoObject;

		public string corruptObjectNoTagName;

		public string corruptObjectNoTaggerBadHeader;

		public string corruptObjectNoTaggerHeader;

		public string corruptObjectNoType;

		public string corruptObjectNotree;

		public string corruptObjectPackfileChecksumIncorrect;

		public string corruptionDetectedReReadingAt;

		public string couldNotCheckOutBecauseOfConflicts;

		public string couldNotDeleteLockFileShouldNotHappen;

		public string couldNotDeleteTemporaryIndexFileShouldNotHappen;

		public string couldNotLockHEAD;

		public string couldNotReadIndexInOneGo;

		public string couldNotRenameDeleteOldIndex;

		public string couldNotRenameTemporaryFile;

		public string couldNotRenameTemporaryIndexFileToIndex;

		public string couldNotURLEncodeToUTF8;

		public string couldNotWriteFile;

		public string countingObjects;

		public string creatingDeltasIsNotImplemented;

		public string daemonAlreadyRunning;

		public string deletingNotSupported;

		public string destinationIsNotAWildcard;

		public string dirCacheDoesNotHaveABackingFile;

		public string dirCacheFileIsNotLocked;

		public string dirCacheIsNotLocked;

		public string dirtyFilesExist;

		public string doesNotHandleMode;

		public string downloadCancelled;

		public string downloadCancelledDuringIndexing;

		public string duplicateAdvertisementsOf;

		public string duplicateRef;

		public string duplicateRemoteRefUpdateIsIllegal;

		public string duplicateStagesNotAllowed;

		public string eitherGitDirOrWorkTreeRequired;

		public string emptyPathNotPermitted;

		public string encryptionError;

		public string endOfFileInEscape;

		public string entryNotFoundByPath;

		public string enumValueNotSupported2;

		public string enumValueNotSupported3;

		public string enumValuesNotAvailable;

		public string errorDecodingFromFile;

		public string errorEncodingFromFile;

		public string errorInBase64CodeReadingStream;

		public string errorInPackedRefs;

		public string errorInvalidProtocolWantedOldNewRef;

		public string errorListing;

		public string errorOccurredDuringUnpackingOnTheRemoteEnd;

		public string errorReadingInfoRefs;

		public string exceptionCaughtDuringExecutionOfAddCommand;

		public string exceptionCaughtDuringExecutionOfCommitCommand;

		public string exceptionCaughtDuringExecutionOfFetchCommand;

		public string exceptionCaughtDuringExecutionOfMergeCommand;

		public string exceptionCaughtDuringExecutionOfPushCommand;

		public string exceptionCaughtDuringExecutionOfTagCommand;

		public string exceptionOccuredDuringAddingOfOptionToALogCommand;

		public string exceptionOccuredDuringReadingOfGIT_DIR;

		public string expectedACKNAKFoundEOF;

		public string expectedACKNAKGot;

		public string expectedBooleanStringValue;

		public string expectedCharacterEncodingGuesses;

		public string expectedEOFReceived;

		public string expectedGot;

		public string expectedPktLineWithService;

		public string expectedReceivedContentType;

		public string expectedReportForRefNotReceived;

		public string failedUpdatingRefs;

		public string failureDueToOneOfTheFollowing;

		public string failureUpdatingFETCH_HEAD;

		public string failureUpdatingTrackingRef;

		public string fileCannotBeDeleted;

		public string fileIsTooBigForThisConvenienceMethod;

		public string fileIsTooLarge;

		public string fileModeNotSetForPath;

		public string flagIsDisposed;

		public string flagNotFromThis;

		public string flagsAlreadyCreated;

		public string funnyRefname;

		public string hugeIndexesAreNotSupportedByJgitYet;

		public string hunkBelongsToAnotherFile;

		public string hunkDisconnectedFromFile;

		public string hunkHeaderDoesNotMatchBodyLineCountOf;

		public string illegalArgumentNotA;

		public string illegalStateExists;

		public string improperlyPaddedBase64Input;

		public string inMemoryBufferLimitExceeded;

		public string incorrectHashFor;

		public string incorrectOBJECT_ID_LENGTH;

		public string incorrectObjectType_COMMITnorTREEnorBLOBnorTAG;

		public string indexFileIsInUse;

		public string indexFileIsTooLargeForJgit;

		public string indexSignatureIsInvalid;

		public string indexWriteException;

		public string integerValueOutOfRange;

		public string internalRevisionError;

		public string interruptedWriting;

		public string invalidAdvertisementOf;

		public string invalidAncestryLength;

		public string invalidBooleanValue;

		public string invalidChannel;

		public string invalidCharacterInBase64Data;

		public string invalidCommitParentNumber;

		public string invalidEncryption;

		public string invalidGitType;

		public string invalidId;

		public string invalidIdLength;

		public string invalidIntegerValue;

		public string invalidKey;

		public string invalidLineInConfigFile;

		public string invalidModeFor;

		public string invalidModeForPath;

		public string invalidObject;

		public string invalidOldIdSent;

		public string invalidPacketLineHeader;

		public string invalidPath;

		public string invalidRemote;

		public string invalidRefName;

		public string invalidStageForPath;

		public string invalidTagOption;

		public string invalidTimeout;

		public string invalidURL;

		public string invalidWildcards;

		public string invalidWindowSize;

		public string isAStaticFlagAndHasNorevWalkInstance;

		public string kNotInRange;

		public string largeObjectException;

		public string largeObjectOutOfMemory;

		public string largeObjectExceedsByteArray;

		public string largeObjectExceedsLimit;

		public string lengthExceedsMaximumArraySize;

		public string listingAlternates;

		public string localObjectsIncomplete;

		public string localRefIsMissingObjects;

		public string lockCountMustBeGreaterOrEqual1;

		public string lockError;

		public string lockOnNotClosed;

		public string lockOnNotHeld;

		public string malformedpersonIdentString;

		public string mergeStrategyAlreadyExistsAsDefault;

		public string mergeStrategyDoesNotSupportHeads;

		public string mergeUsingStrategyResultedInDescription;

		public string missingAccesskey;

		public string missingDeltaBase;

		public string missingForwardImageInGITBinaryPatch;

		public string missingObject;

		public string missingPrerequisiteCommits;

		public string missingSecretkey;

		public string mixedStagesNotAllowed;

		public string multipleMergeBasesFor;

		public string need2Arguments;

		public string needPackOut;

		public string needsAtLeastOneEntry;

		public string needsWorkdir;

		public string newlineInQuotesNotAllowed;

		public string noApplyInDelete;

		public string noClosingBracket;

		public string noHEADExistsAndNoExplicitStartingRevisionWasSpecified;

		public string noHMACsupport;

		public string noMergeHeadSpecified;

		public string noSuchRef;

		public string noXMLParserAvailable;

		public string notABoolean;

		public string notABundle;

		public string notADIRCFile;

		public string notAGitDirectory;

		public string notAPACKFile;

		public string notARef;

		public string notASCIIString;

		public string notAValidPack;

		public string notFound;

		public string notValid;

		public string nothingToFetch;

		public string nothingToPush;

		public string objectAtHasBadZlibStream;

		public string objectAtPathDoesNotHaveId;

		public string objectIsCorrupt;

		public string objectIsNotA;

		public string objectNotFoundIn;

		public string offsetWrittenDeltaBaseForObjectNotFoundInAPack;

		public string onlyAlreadyUpToDateAndFastForwardMergesAreAvailable;

		public string onlyOneFetchSupported;

		public string onlyOneOperationCallPerConnectionIsSupported;

		public string openFilesMustBeAtLeast1;

		public string openingConnection;

		public string outputHasAlreadyBeenStarted;

		public string packChecksumMismatch;

		public string packCorruptedWhileWritingToFilesystem;

		public string packDoesNotMatchIndex;

		public string packFileInvalid;

		public string packHasUnresolvedDeltas;

		public string packObjectCountMismatch;

		public string packTooLargeForIndexVersion1;

		public string packetSizeMustBeAtLeast;

		public string packetSizeMustBeAtMost;

		public string packfileCorruptionDetected;

		public string packfileIsTruncated;

		public string packingCancelledDuringObjectsWriting;

		public string pathIsNotInWorkingDir;

		public string peeledLineBeforeRef;

		public string peerDidNotSupplyACompleteObjectGraph;

		public string prefixRemote;

		public string problemWithResolvingPushRefSpecsLocally;

		public string progressMonUploading;

		public string propertyIsAlreadyNonNull;

		public string pushCancelled;

		public string pushIsNotSupportedForBundleTransport;

		public string pushNotPermitted;

		public string rawLogMessageDoesNotParseAsLogEntry;

		public string readTimedOut;

		public string readingObjectsFromLocalRepositoryFailed;

		public string receivingObjects;

		public string refUpdateReturnCodeWas;

		public string reflogsNotYetSupportedByRevisionParser;

		public string remoteConfigHasNoURIAssociated;

		public string remoteDoesNotHaveSpec;

		public string remoteDoesNotSupportSmartHTTPPush;

		public string remoteHungUpUnexpectedly;

		public string remoteNameCantBeNull;

		public string renamesAlreadyFound;

		public string renamesBreakingModifies;

		public string renamesFindingByContent;

		public string renamesFindingExact;

		public string renamesRejoiningModifies;

		public string repositoryAlreadyExists;

		public string repositoryConfigFileInvalid;

		public string repositoryIsRequired;

		public string repositoryNotFound;

		public string repositoryState_applyMailbox;

		public string repositoryState_bisecting;

		public string repositoryState_conflicts;

		public string repositoryState_merged;

		public string repositoryState_normal;

		public string repositoryState_rebase;

		public string repositoryState_rebaseInteractive;

		public string repositoryState_rebaseOrApplyMailbox;

		public string repositoryState_rebaseWithMerge;

		public string requiredHashFunctionNotAvailable;

		public string resolvingDeltas;

		public string resultLengthIncorrect;

		public string searchForReuse;

		public string sequenceTooLargeForDiffAlgorithm;

		public string serviceNotPermitted;

		public string shortCompressedStreamAt;

		public string shortReadOfBlock;

		public string shortReadOfOptionalDIRCExtensionExpectedAnotherBytes;

		public string shortSkipOfBlock;

		public string signingNotSupportedOnTag;

		public string similarityScoreMustBeWithinBounds;

		public string sizeExceeds2GB;

		public string smartHTTPPushDisabled;

		public string sourceDestinationMustMatch;

		public string sourceIsNotAWildcard;

		public string sourceRefDoesntResolveToAnyObject;

		public string sourceRefNotSpecifiedForRefspec;

		public string staleRevFlagsOn;

		public string startingReadStageWithoutWrittenRequestDataPendingIsNotSupported;

		public string statelessRPCRequiresOptionToBeEnabled;

		public string submodulesNotSupported;

		public string symlinkCannotBeWrittenAsTheLinkTarget;

		public string tagNameInvalid;

		public string tagOnRepoWithoutHEADCurrentlyNotSupported;

		public string tSizeMustBeGreaterOrEqual1;

		public string theFactoryMustNotBeNull;

		public string timerAlreadyTerminated;

		public string topologicalSortRequired;

		public string transportExceptionBadRef;

		public string transportExceptionEmptyRef;

		public string transportExceptionInvalid;

		public string transportExceptionMissingAssumed;

		public string transportExceptionReadRef;

		public string treeEntryAlreadyExists;

		public string treeIteratorDoesNotSupportRemove;

		public string truncatedHunkLinesMissingForAncestor;

		public string truncatedHunkNewLinesMissing;

		public string truncatedHunkOldLinesMissing;

		public string unableToCheckConnectivity;

		public string unableToStore;

		public string unableToWrite;

		public string unencodeableFile;

		public string unexpectedEndOfConfigFile;

		public string unexpectedHunkTrailer;

		public string unexpectedOddResult;

		public string unexpectedRefReport;

		public string unexpectedReportLine2;

		public string unexpectedReportLine;

		public string unknownDIRCVersion;

		public string unknownHost;

		public string unknownIndexVersionOrCorruptIndex;

		public string unknownObject;

		public string unknownObjectType;

		public string unknownRepositoryFormat2;

		public string unknownRepositoryFormat;

		public string unknownZlibError;

		public string unmergedPath;

		public string unpackException;

		public string unreadablePackIndex;

		public string unrecognizedRef;

		public string unsupportedCommand0;

		public string unsupportedEncryptionAlgorithm;

		public string unsupportedEncryptionVersion;

		public string unsupportedOperationNotAddAtEnd;

		public string unsupportedPackIndexVersion;

		public string unsupportedPackVersion;

		public string updatingRefFailed;

		public string userConfigFileInvalid;

		public string walkFailure;

		public string windowSizeMustBeLesserThanLimit;

		public string windowSizeMustBePowerOf2;

		public string writeTimedOut;

		public string writerAlreadyInitialized;

		public string writingNotPermitted;

		public string writingNotSupported;

		public string writingObjects;

		public string wrongDecompressedLength;
	}
}
