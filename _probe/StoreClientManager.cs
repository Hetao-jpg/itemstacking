using System;
using System.Runtime.CompilerServices;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;

namespace Il2Cpp;

public class StoreClientManager : Il2CppSystem.Object
{
	[ObfuscatedName("StoreClientManager+<>c__DisplayClass47_0")]
	public sealed class __c__DisplayClass47_0 : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_seenIdentifiers;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__ClientGenCheck_b__0_Internal_Boolean_StoreClient_0;

		public unsafe HashSet<string> seenIdentifiers
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_seenIdentifiers);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<HashSet<string>>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_seenIdentifiers)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)hashSet));
			}
		}

		static __c__DisplayClass47_0()
		{
			Il2CppClassPointerStore<__c__DisplayClass47_0>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "<>c__DisplayClass47_0");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c__DisplayClass47_0>.NativeClassPtr);
			NativeFieldInfoPtr_seenIdentifiers = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass47_0>.NativeClassPtr, "seenIdentifiers");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass47_0>.NativeClassPtr, 100671694);
			NativeMethodInfoPtr__ClientGenCheck_b__0_Internal_Boolean_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass47_0>.NativeClassPtr, 100671695);
		}

		[CallerCount(1403)]
		[CachedScanResults(RefRangeStart = 1155, RefRangeEnd = 2558, XrefRangeStart = 1155, XrefRangeEnd = 2558, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe __c__DisplayClass47_0()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c__DisplayClass47_0>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300956, XrefRangeEnd = 300965, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _ClientGenCheck_b__0(StoreClient client)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)client);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ClientGenCheck_b__0_Internal_Boolean_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		public __c__DisplayClass47_0(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	[ObfuscatedName("StoreClientManager+<>c__DisplayClass49_0")]
	public sealed class __c__DisplayClass49_0 : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_id;

		private static readonly System.IntPtr NativeFieldInfoPtr_firstInstanceFound;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__RemoveDuplicateClientsByIdentifier_b__0_Internal_Boolean_StoreClient_0;

		public unsafe string id
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_id);
				return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_id)), IL2CPP.ManagedStringToIl2Cpp(text));
			}
		}

		public unsafe bool firstInstanceFound
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_firstInstanceFound);
				return *(bool*)num;
			}
			set
			{
				*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_firstInstanceFound)) = flag;
			}
		}

		static __c__DisplayClass49_0()
		{
			Il2CppClassPointerStore<__c__DisplayClass49_0>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "<>c__DisplayClass49_0");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c__DisplayClass49_0>.NativeClassPtr);
			NativeFieldInfoPtr_id = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass49_0>.NativeClassPtr, "id");
			NativeFieldInfoPtr_firstInstanceFound = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass49_0>.NativeClassPtr, "firstInstanceFound");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass49_0>.NativeClassPtr, 100671696);
			NativeMethodInfoPtr__RemoveDuplicateClientsByIdentifier_b__0_Internal_Boolean_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass49_0>.NativeClassPtr, 100671697);
		}

		[CallerCount(1403)]
		[CachedScanResults(RefRangeStart = 1155, RefRangeEnd = 2558, XrefRangeStart = 1155, XrefRangeEnd = 2558, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe __c__DisplayClass49_0()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c__DisplayClass49_0>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300965, XrefRangeEnd = 300966, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _RemoveDuplicateClientsByIdentifier_b__0(StoreClient client)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)client);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__RemoveDuplicateClientsByIdentifier_b__0_Internal_Boolean_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		public __c__DisplayClass49_0(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	[ObfuscatedName("StoreClientManager+<>c__DisplayClass54_0")]
	public sealed class __c__DisplayClass54_0 : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_storeClient;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__TryAddClient_b__0_Internal_Boolean_StoreClient_0;

		public unsafe StoreClient storeClient
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_storeClient);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_storeClient)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient));
			}
		}

		static __c__DisplayClass54_0()
		{
			Il2CppClassPointerStore<__c__DisplayClass54_0>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "<>c__DisplayClass54_0");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c__DisplayClass54_0>.NativeClassPtr);
			NativeFieldInfoPtr_storeClient = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass54_0>.NativeClassPtr, "storeClient");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass54_0>.NativeClassPtr, 100671698);
			NativeMethodInfoPtr__TryAddClient_b__0_Internal_Boolean_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass54_0>.NativeClassPtr, 100671699);
		}

		[CallerCount(1403)]
		[CachedScanResults(RefRangeStart = 1155, RefRangeEnd = 2558, XrefRangeStart = 1155, XrefRangeEnd = 2558, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe __c__DisplayClass54_0()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c__DisplayClass54_0>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300966, XrefRangeEnd = 300968, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _TryAddClient_b__0(StoreClient c)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)c);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__TryAddClient_b__0_Internal_Boolean_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		public __c__DisplayClass54_0(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	[ObfuscatedName("StoreClientManager+<>c__DisplayClass86_0")]
	public sealed class __c__DisplayClass86_0 : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr_threshold;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__HandleMarketClient_b__0_Internal_Boolean_KeyValuePair_2_String_Int32_0;

		public unsafe int threshold
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_threshold);
				return *(int*)num;
			}
			set
			{
				*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_threshold)) = num;
			}
		}

		static __c__DisplayClass86_0()
		{
			Il2CppClassPointerStore<__c__DisplayClass86_0>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "<>c__DisplayClass86_0");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c__DisplayClass86_0>.NativeClassPtr);
			NativeFieldInfoPtr_threshold = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass86_0>.NativeClassPtr, "threshold");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass86_0>.NativeClassPtr, 100671700);
			NativeMethodInfoPtr__HandleMarketClient_b__0_Internal_Boolean_KeyValuePair_2_String_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass86_0>.NativeClassPtr, 100671701);
		}

		[CallerCount(1403)]
		[CachedScanResults(RefRangeStart = 1155, RefRangeEnd = 2558, XrefRangeStart = 1155, XrefRangeEnd = 2558, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe __c__DisplayClass86_0()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c__DisplayClass86_0>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300968, XrefRangeEnd = 300969, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _HandleMarketClient_b__0(KeyValuePair<string, int> tv)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)tv));
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__HandleMarketClient_b__0_Internal_Boolean_KeyValuePair_2_String_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		public __c__DisplayClass86_0(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	[System.Serializable]
	[ObfuscatedName("StoreClientManager+<>c")]
	public sealed class __c : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr___9;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__109_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__FindEventCustomerAugCandidate_b__109_0_Internal_Boolean_StoreClient_0;

		public unsafe static __c __9
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<__c>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)_c));
			}
		}

		public unsafe static Il2CppSystem.Func<StoreClient, bool> __9__109_0
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__109_0, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<StoreClient, bool>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__109_0, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
			}
		}

		static __c()
		{
			Il2CppClassPointerStore<__c>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "<>c");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c>.NativeClassPtr);
			NativeFieldInfoPtr___9 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9");
			NativeFieldInfoPtr___9__109_0 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__109_0");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100671703);
			NativeMethodInfoPtr__FindEventCustomerAugCandidate_b__109_0_Internal_Boolean_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100671704);
		}

		[CallerCount(1403)]
		[CachedScanResults(RefRangeStart = 1155, RefRangeEnd = 2558, XrefRangeStart = 1155, XrefRangeEnd = 2558, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe __c()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300969, XrefRangeEnd = 300982, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe bool _FindEventCustomerAugCandidate_b__109_0(StoreClient c)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)c);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__FindEventCustomerAugCandidate_b__109_0_Internal_Boolean_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
		}

		public __c(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	private static readonly System.IntPtr NativeFieldInfoPtr_clientStack;

	private static readonly System.IntPtr NativeFieldInfoPtr_chemicalFeedbackStack;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientTracker;

	private static readonly System.IntPtr NativeFieldInfoPtr_permanentClientTracker;

	private static readonly System.IntPtr NativeFieldInfoPtr_uniqueClientTracker;

	private static readonly System.IntPtr NativeFieldInfoPtr_arrestedClients;

	private static readonly System.IntPtr NativeFieldInfoPtr_possibleRandomWantedClient;

	private static readonly System.IntPtr NativeFieldInfoPtr_isTodayWantedQueued;

	private static readonly System.IntPtr NativeFieldInfoPtr_daySinceWantedWasQueued;

	private static readonly System.IntPtr NativeFieldInfoPtr_isAugSpawnHandledToday;

	private static readonly System.IntPtr NativeFieldInfoPtr_DAY_UNTIL_NORMALE_CLIENT_SPAWN;

	private static readonly System.IntPtr NativeFieldInfoPtr_DAY_UNTIL_INTEL_CLIENT_SPAWN;

	private static readonly System.IntPtr NativeFieldInfoPtr_DAY_UNTIL_MINOR_CLIENT_SPAWN;

	private static readonly System.IntPtr NativeFieldInfoPtr_daySinceInformationClient;

	private static readonly System.IntPtr NativeFieldInfoPtr_daySinceInspection;

	private static readonly System.IntPtr NativeFieldInfoPtr_dayUntilInspection;

	private static readonly System.IntPtr NativeFieldInfoPtr_inspectionSeeded;

	private static readonly System.IntPtr NativeFieldInfoPtr_INSPECTION_TRUSTED_BONUS_MIN;

	private static readonly System.IntPtr NativeFieldInfoPtr_INSPECTION_TRUSTED_BONUS_MAX;

	private static readonly System.IntPtr NativeFieldInfoPtr_INSPECTION_MIN_TRAFFICKING_CRIME;

	private static readonly System.IntPtr NativeFieldInfoPtr_MINIMUM_ATT_DISPLAY_CASE;

	private static readonly System.IntPtr NativeFieldInfoPtr_ONE_NORMAL_ATT_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_TWO_NORMAL_ATT_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_THREE_NORMAL_ATT_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_EXTRA_BUDGET_10_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_EXTRA_BUDGET_25_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_EXTRA_BUDGET_50_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_EXTRA_BUDGET_75_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_EXTRA_BUDGET_100_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_RICH_CUSTOMER_400_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_RICH_CUSTOMER_650_THRESHOLD;

	private static readonly System.IntPtr NativeFieldInfoPtr_WILD_BAILOUT_DECLINE_DAYS;

	private static readonly System.IntPtr NativeFieldInfoPtr_WILD_BAILOUT_TOLERANCE_DAYS;

	private static readonly System.IntPtr NativeFieldInfoPtr_WILD_BAILOUT_WINDOW;

	private static readonly System.IntPtr NativeFieldInfoPtr_wildBailoutGiven;

	private static readonly System.IntPtr NativeFieldInfoPtr_wealthHistory;

	private static readonly System.IntPtr NativeFieldInfoPtr_WANTED_INTRO_CLIENT_ID;

	private static readonly System.IntPtr NativeFieldInfoPtr_AUG_DISGUISE_CHANCE;

	private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GenerateClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnNewDay_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_PickClient_Public_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_PickRandomClient_Public_StoreClient_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_PickRandomValidClient_Public_StoreClient_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_PickShopperClient_Public_Static_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_PickSupplierClient_Public_Static_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsClientValid_Public_Static_Boolean_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsClientOnCooldown_Public_Static_Boolean_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsClientOnCooldown_Public_Static_Boolean_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ClientGenCheck_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_RemoveDuplicateClientsByIdentifier_Public_Void_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AddClient_Public_Void_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsClientBlockedByKnownSnitch_Public_Static_Boolean_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_TryAddClient_Public_Void_StoreClient_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AddNextClient_Public_Void_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckRentDay_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleSkipClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleServiceClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ComputeInspectionCooldownRange_Private_Void_byref_Int32_byref_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ResetInspection_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsInspectionCountdownActive_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleInspectionClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleShowCaseSecurity_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleDebugClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleSawyerCrewClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleAlwayClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleCharacterLine_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleImportantScriptedClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleWildBailout_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_UpdateWealthTrend_Private_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsWealthTrendingDown_Private_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleClientFromIntro_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleClientFromEvent_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_UpdateTracker_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_TrackClient_Public_Void_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleShowcaseClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandlePerkClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleNormalClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetNormalCustomerCount_Public_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleInformationClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleSupplierClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleInventorClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleMarketClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_DismissClientStack_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetRandomNewMinorClient_Public_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleMinorClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetRandomValidWantedClient_Public_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetRandomClientSpawnChance_Public_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleContentUnlockClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleChemicalFeedbackClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckStoryLineGeorgio_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckStoryLineOfficerLun_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckStoryLineEleanor_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckStoryLineNellis_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CheckStoryLineSerpico_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleBlackmailClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleJacksonStorage_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ComputeClientType_Public_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsCharityComing_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetCustomerTotalVisitedCount_Public_Static_Int32_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleExpeditionClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HandleAugClient_Public_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AddAugClient_Private_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_FindEventCustomerAugCandidate_Private_StoreClient_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__GetRandomValidWantedClient_b__91_0_Private_Boolean_String_0;

	public unsafe List<StoreClient> clientStack
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientStack);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<StoreClient>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientStack)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe List<StoreClient> chemicalFeedbackStack
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_chemicalFeedbackStack);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<StoreClient>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_chemicalFeedbackStack)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe Dictionary<string, int> clientTracker
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientTracker);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<string, int>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientTracker)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe Dictionary<string, int> permanentClientTracker
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_permanentClientTracker);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<string, int>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_permanentClientTracker)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe Dictionary<string, int> uniqueClientTracker
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_uniqueClientTracker);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dictionary<string, int>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_uniqueClientTracker)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dictionary));
		}
	}

	public unsafe List<string> arrestedClients
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_arrestedClients);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_arrestedClients)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe List<string> possibleRandomWantedClient
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_possibleRandomWantedClient);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_possibleRandomWantedClient)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe bool isTodayWantedQueued
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isTodayWantedQueued);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isTodayWantedQueued)) = flag;
		}
	}

	public unsafe int daySinceWantedWasQueued
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_daySinceWantedWasQueued);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_daySinceWantedWasQueued)) = num;
		}
	}

	public unsafe bool isAugSpawnHandledToday
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isAugSpawnHandledToday);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isAugSpawnHandledToday)) = flag;
		}
	}

	public unsafe static int DAY_UNTIL_NORMALE_CLIENT_SPAWN
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_DAY_UNTIL_NORMALE_CLIENT_SPAWN, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_DAY_UNTIL_NORMALE_CLIENT_SPAWN, (void*)(&num));
		}
	}

	public unsafe static int DAY_UNTIL_INTEL_CLIENT_SPAWN
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_DAY_UNTIL_INTEL_CLIENT_SPAWN, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_DAY_UNTIL_INTEL_CLIENT_SPAWN, (void*)(&num));
		}
	}

	public unsafe static int DAY_UNTIL_MINOR_CLIENT_SPAWN
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_DAY_UNTIL_MINOR_CLIENT_SPAWN, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_DAY_UNTIL_MINOR_CLIENT_SPAWN, (void*)(&num));
		}
	}

	public unsafe int daySinceInformationClient
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_daySinceInformationClient);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_daySinceInformationClient)) = num;
		}
	}

	public unsafe int daySinceInspection
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_daySinceInspection);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_daySinceInspection)) = num;
		}
	}

	public unsafe int dayUntilInspection
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_dayUntilInspection);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_dayUntilInspection)) = num;
		}
	}

	public unsafe bool inspectionSeeded
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_inspectionSeeded);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_inspectionSeeded)) = flag;
		}
	}

	public unsafe static int INSPECTION_TRUSTED_BONUS_MIN
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_INSPECTION_TRUSTED_BONUS_MIN, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_INSPECTION_TRUSTED_BONUS_MIN, (void*)(&num));
		}
	}

	public unsafe static int INSPECTION_TRUSTED_BONUS_MAX
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_INSPECTION_TRUSTED_BONUS_MAX, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_INSPECTION_TRUSTED_BONUS_MAX, (void*)(&num));
		}
	}

	public unsafe static int INSPECTION_MIN_TRAFFICKING_CRIME
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_INSPECTION_MIN_TRAFFICKING_CRIME, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_INSPECTION_MIN_TRAFFICKING_CRIME, (void*)(&num));
		}
	}

	public unsafe static int MINIMUM_ATT_DISPLAY_CASE
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_MINIMUM_ATT_DISPLAY_CASE, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_MINIMUM_ATT_DISPLAY_CASE, (void*)(&num));
		}
	}

	public unsafe static int ONE_NORMAL_ATT_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_ONE_NORMAL_ATT_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_ONE_NORMAL_ATT_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int TWO_NORMAL_ATT_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_TWO_NORMAL_ATT_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_TWO_NORMAL_ATT_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int THREE_NORMAL_ATT_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_THREE_NORMAL_ATT_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_THREE_NORMAL_ATT_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int EXTRA_BUDGET_10_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_EXTRA_BUDGET_10_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_EXTRA_BUDGET_10_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int EXTRA_BUDGET_25_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_EXTRA_BUDGET_25_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_EXTRA_BUDGET_25_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int EXTRA_BUDGET_50_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_EXTRA_BUDGET_50_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_EXTRA_BUDGET_50_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int EXTRA_BUDGET_75_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_EXTRA_BUDGET_75_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_EXTRA_BUDGET_75_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int EXTRA_BUDGET_100_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_EXTRA_BUDGET_100_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_EXTRA_BUDGET_100_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int RICH_CUSTOMER_400_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_RICH_CUSTOMER_400_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_RICH_CUSTOMER_400_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int RICH_CUSTOMER_650_THRESHOLD
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_RICH_CUSTOMER_650_THRESHOLD, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_RICH_CUSTOMER_650_THRESHOLD, (void*)(&num));
		}
	}

	public unsafe static int WILD_BAILOUT_DECLINE_DAYS
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_WILD_BAILOUT_DECLINE_DAYS, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_WILD_BAILOUT_DECLINE_DAYS, (void*)(&num));
		}
	}

	public unsafe static int WILD_BAILOUT_TOLERANCE_DAYS
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_WILD_BAILOUT_TOLERANCE_DAYS, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_WILD_BAILOUT_TOLERANCE_DAYS, (void*)(&num));
		}
	}

	public unsafe static int WILD_BAILOUT_WINDOW
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_WILD_BAILOUT_WINDOW, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_WILD_BAILOUT_WINDOW, (void*)(&num));
		}
	}

	public unsafe bool wildBailoutGiven
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_wildBailoutGiven);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_wildBailoutGiven)) = flag;
		}
	}

	public unsafe List<int> wealthHistory
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_wealthHistory);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<int>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_wealthHistory)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe static string WANTED_INTRO_CLIENT_ID
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_WANTED_INTRO_CLIENT_ID, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_WANTED_INTRO_CLIENT_ID, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static int AUG_DISGUISE_CHANCE
	{
		get
		{
			Unsafe.SkipInit(out int result);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_AUG_DISGUISE_CHANCE, (void*)(&result));
			return result;
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_AUG_DISGUISE_CHANCE, (void*)(&num));
		}
	}

	static StoreClientManager()
	{
		Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr = IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "", "StoreClientManager");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr);
		NativeFieldInfoPtr_clientStack = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "clientStack");
		NativeFieldInfoPtr_chemicalFeedbackStack = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "chemicalFeedbackStack");
		NativeFieldInfoPtr_clientTracker = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "clientTracker");
		NativeFieldInfoPtr_permanentClientTracker = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "permanentClientTracker");
		NativeFieldInfoPtr_uniqueClientTracker = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "uniqueClientTracker");
		NativeFieldInfoPtr_arrestedClients = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "arrestedClients");
		NativeFieldInfoPtr_possibleRandomWantedClient = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "possibleRandomWantedClient");
		NativeFieldInfoPtr_isTodayWantedQueued = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "isTodayWantedQueued");
		NativeFieldInfoPtr_daySinceWantedWasQueued = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "daySinceWantedWasQueued");
		NativeFieldInfoPtr_isAugSpawnHandledToday = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "isAugSpawnHandledToday");
		NativeFieldInfoPtr_DAY_UNTIL_NORMALE_CLIENT_SPAWN = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "DAY_UNTIL_NORMALE_CLIENT_SPAWN");
		NativeFieldInfoPtr_DAY_UNTIL_INTEL_CLIENT_SPAWN = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "DAY_UNTIL_INTEL_CLIENT_SPAWN");
		NativeFieldInfoPtr_DAY_UNTIL_MINOR_CLIENT_SPAWN = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "DAY_UNTIL_MINOR_CLIENT_SPAWN");
		NativeFieldInfoPtr_daySinceInformationClient = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "daySinceInformationClient");
		NativeFieldInfoPtr_daySinceInspection = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "daySinceInspection");
		NativeFieldInfoPtr_dayUntilInspection = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "dayUntilInspection");
		NativeFieldInfoPtr_inspectionSeeded = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "inspectionSeeded");
		NativeFieldInfoPtr_INSPECTION_TRUSTED_BONUS_MIN = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "INSPECTION_TRUSTED_BONUS_MIN");
		NativeFieldInfoPtr_INSPECTION_TRUSTED_BONUS_MAX = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "INSPECTION_TRUSTED_BONUS_MAX");
		NativeFieldInfoPtr_INSPECTION_MIN_TRAFFICKING_CRIME = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "INSPECTION_MIN_TRAFFICKING_CRIME");
		NativeFieldInfoPtr_MINIMUM_ATT_DISPLAY_CASE = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "MINIMUM_ATT_DISPLAY_CASE");
		NativeFieldInfoPtr_ONE_NORMAL_ATT_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "ONE_NORMAL_ATT_THRESHOLD");
		NativeFieldInfoPtr_TWO_NORMAL_ATT_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "TWO_NORMAL_ATT_THRESHOLD");
		NativeFieldInfoPtr_THREE_NORMAL_ATT_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "THREE_NORMAL_ATT_THRESHOLD");
		NativeFieldInfoPtr_EXTRA_BUDGET_10_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "EXTRA_BUDGET_10_THRESHOLD");
		NativeFieldInfoPtr_EXTRA_BUDGET_25_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "EXTRA_BUDGET_25_THRESHOLD");
		NativeFieldInfoPtr_EXTRA_BUDGET_50_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "EXTRA_BUDGET_50_THRESHOLD");
		NativeFieldInfoPtr_EXTRA_BUDGET_75_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "EXTRA_BUDGET_75_THRESHOLD");
		NativeFieldInfoPtr_EXTRA_BUDGET_100_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "EXTRA_BUDGET_100_THRESHOLD");
		NativeFieldInfoPtr_RICH_CUSTOMER_400_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "RICH_CUSTOMER_400_THRESHOLD");
		NativeFieldInfoPtr_RICH_CUSTOMER_650_THRESHOLD = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "RICH_CUSTOMER_650_THRESHOLD");
		NativeFieldInfoPtr_WILD_BAILOUT_DECLINE_DAYS = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "WILD_BAILOUT_DECLINE_DAYS");
		NativeFieldInfoPtr_WILD_BAILOUT_TOLERANCE_DAYS = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "WILD_BAILOUT_TOLERANCE_DAYS");
		NativeFieldInfoPtr_WILD_BAILOUT_WINDOW = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "WILD_BAILOUT_WINDOW");
		NativeFieldInfoPtr_wildBailoutGiven = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "wildBailoutGiven");
		NativeFieldInfoPtr_wealthHistory = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "wealthHistory");
		NativeFieldInfoPtr_WANTED_INTRO_CLIENT_ID = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "WANTED_INTRO_CLIENT_ID");
		NativeFieldInfoPtr_AUG_DISGUISE_CHANCE = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, "AUG_DISGUISE_CHANCE");
		NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671627);
		NativeMethodInfoPtr_GenerateClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671628);
		NativeMethodInfoPtr_OnNewDay_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671629);
		NativeMethodInfoPtr_PickClient_Public_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671630);
		NativeMethodInfoPtr_PickRandomClient_Public_StoreClient_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671631);
		NativeMethodInfoPtr_PickRandomValidClient_Public_StoreClient_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671632);
		NativeMethodInfoPtr_PickShopperClient_Public_Static_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671633);
		NativeMethodInfoPtr_PickSupplierClient_Public_Static_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671634);
		NativeMethodInfoPtr_IsClientValid_Public_Static_Boolean_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671635);
		NativeMethodInfoPtr_IsClientOnCooldown_Public_Static_Boolean_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671636);
		NativeMethodInfoPtr_IsClientOnCooldown_Public_Static_Boolean_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671637);
		NativeMethodInfoPtr_ClientGenCheck_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671638);
		NativeMethodInfoPtr_RemoveDuplicateClientsByIdentifier_Public_Void_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671639);
		NativeMethodInfoPtr_AddClient_Public_Void_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671640);
		NativeMethodInfoPtr_IsClientBlockedByKnownSnitch_Public_Static_Boolean_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671641);
		NativeMethodInfoPtr_TryAddClient_Public_Void_StoreClient_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671642);
		NativeMethodInfoPtr_AddNextClient_Public_Void_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671643);
		NativeMethodInfoPtr_CheckRentDay_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671644);
		NativeMethodInfoPtr_HandleSkipClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671645);
		NativeMethodInfoPtr_HandleServiceClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671646);
		NativeMethodInfoPtr_ComputeInspectionCooldownRange_Private_Void_byref_Int32_byref_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671647);
		NativeMethodInfoPtr_ResetInspection_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671648);
		NativeMethodInfoPtr_IsInspectionCountdownActive_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671649);
		NativeMethodInfoPtr_HandleInspectionClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671650);
		NativeMethodInfoPtr_HandleShowCaseSecurity_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671651);
		NativeMethodInfoPtr_HandleDebugClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671652);
		NativeMethodInfoPtr_HandleSawyerCrewClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671653);
		NativeMethodInfoPtr_HandleAlwayClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671654);
		NativeMethodInfoPtr_HandleCharacterLine_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671655);
		NativeMethodInfoPtr_HandleImportantScriptedClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671656);
		NativeMethodInfoPtr_HandleWildBailout_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671657);
		NativeMethodInfoPtr_UpdateWealthTrend_Private_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671658);
		NativeMethodInfoPtr_IsWealthTrendingDown_Private_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671659);
		NativeMethodInfoPtr_HandleClientFromIntro_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671660);
		NativeMethodInfoPtr_HandleClientFromEvent_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671661);
		NativeMethodInfoPtr_UpdateTracker_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671662);
		NativeMethodInfoPtr_TrackClient_Public_Void_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671663);
		NativeMethodInfoPtr_HandleShowcaseClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671664);
		NativeMethodInfoPtr_HandlePerkClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671665);
		NativeMethodInfoPtr_HandleNormalClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671666);
		NativeMethodInfoPtr_GetNormalCustomerCount_Public_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671667);
		NativeMethodInfoPtr_HandleInformationClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671668);
		NativeMethodInfoPtr_HandleSupplierClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671669);
		NativeMethodInfoPtr_HandleInventorClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671670);
		NativeMethodInfoPtr_HandleMarketClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671671);
		NativeMethodInfoPtr_DismissClientStack_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671672);
		NativeMethodInfoPtr_GetRandomNewMinorClient_Public_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671673);
		NativeMethodInfoPtr_HandleMinorClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671674);
		NativeMethodInfoPtr_GetRandomValidWantedClient_Public_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671675);
		NativeMethodInfoPtr_GetRandomClientSpawnChance_Public_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671676);
		NativeMethodInfoPtr_HandleContentUnlockClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671677);
		NativeMethodInfoPtr_HandleChemicalFeedbackClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671678);
		NativeMethodInfoPtr_CheckStoryLineGeorgio_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671679);
		NativeMethodInfoPtr_CheckStoryLineOfficerLun_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671680);
		NativeMethodInfoPtr_CheckStoryLineEleanor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671681);
		NativeMethodInfoPtr_CheckStoryLineNellis_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671682);
		NativeMethodInfoPtr_CheckStoryLineSerpico_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671683);
		NativeMethodInfoPtr_HandleBlackmailClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671684);
		NativeMethodInfoPtr_HandleJacksonStorage_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671685);
		NativeMethodInfoPtr_ComputeClientType_Public_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671686);
		NativeMethodInfoPtr_IsCharityComing_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671687);
		NativeMethodInfoPtr_GetCustomerTotalVisitedCount_Public_Static_Int32_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671688);
		NativeMethodInfoPtr_HandleExpeditionClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671689);
		NativeMethodInfoPtr_HandleAugClient_Public_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671690);
		NativeMethodInfoPtr_AddAugClient_Private_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671691);
		NativeMethodInfoPtr_FindEventCustomerAugCandidate_Private_StoreClient_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671692);
		NativeMethodInfoPtr__GetRandomValidWantedClient_b__91_0_Private_Boolean_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr, 100671693);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301061, RefRangeEnd = 301062, XrefRangeStart = 300982, XrefRangeEnd = 301061, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe StoreClientManager()
		: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<StoreClientManager>.NativeClassPtr))
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301210, RefRangeEnd = 301211, XrefRangeStart = 301062, XrefRangeEnd = 301210, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void GenerateClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GenerateClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301211, RefRangeEnd = 301212, XrefRangeStart = 301211, XrefRangeEnd = 301211, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void OnNewDay()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnNewDay_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301271, RefRangeEnd = 301272, XrefRangeStart = 301212, XrefRangeEnd = 301271, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe StoreClient PickClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_PickClient_Public_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 301340, RefRangeEnd = 301344, XrefRangeStart = 301272, XrefRangeEnd = 301340, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe StoreClient PickRandomClient(string faction = "")
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(faction);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_PickRandomClient_Public_StoreClient_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 301344, XrefRangeEnd = 301346, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe StoreClient PickRandomValidClient(string type = "")
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(type);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_PickRandomValidClient_Public_StoreClient_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 301346, XrefRangeEnd = 301366, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static StoreClient PickShopperClient()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_PickShopperClient_Public_Static_StoreClient_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 301379, RefRangeEnd = 301381, XrefRangeStart = 301366, XrefRangeEnd = 301379, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static StoreClient PickSupplierClient()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_PickSupplierClient_Public_Static_StoreClient_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 301381, XrefRangeEnd = 301382, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool IsClientValid(StoreClient storeClient)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsClientValid_Public_Static_Boolean_StoreClient_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(6)]
	[CachedScanResults(RefRangeStart = 301390, RefRangeEnd = 301396, XrefRangeStart = 301382, XrefRangeEnd = 301390, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool IsClientOnCooldown(StoreClient storeClient)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsClientOnCooldown_Public_Static_Boolean_StoreClient_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(5)]
	[CachedScanResults(RefRangeStart = 301404, RefRangeEnd = 301409, XrefRangeStart = 301396, XrefRangeEnd = 301404, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool IsClientOnCooldown(string clientId)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(clientId);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsClientOnCooldown_Public_Static_Boolean_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 301409, XrefRangeEnd = 301436, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ClientGenCheck()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ClientGenCheck_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 301451, RefRangeEnd = 301455, XrefRangeStart = 301436, XrefRangeEnd = 301451, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void RemoveDuplicateClientsByIdentifier(string id)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(id);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_RemoveDuplicateClientsByIdentifier_Public_Void_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(142)]
	[CachedScanResults(RefRangeStart = 301466, RefRangeEnd = 301608, XrefRangeStart = 301455, XrefRangeEnd = 301466, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void AddClient(StoreClient storeClient)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AddClient_Public_Void_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(23)]
	[CachedScanResults(RefRangeStart = 301622, RefRangeEnd = 301645, XrefRangeStart = 301608, XrefRangeEnd = 301622, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static bool IsClientBlockedByKnownSnitch(StoreClient storeClient)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsClientBlockedByKnownSnitch_Public_Static_Boolean_StoreClient_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(9)]
	[CachedScanResults(RefRangeStart = 301663, RefRangeEnd = 301672, XrefRangeStart = 301645, XrefRangeEnd = 301663, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void TryAddClient(StoreClient storeClient, bool noDuplicate = true)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient);
		*(bool**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &noDuplicate;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TryAddClient_Public_Void_StoreClient_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301683, RefRangeEnd = 301684, XrefRangeStart = 301672, XrefRangeEnd = 301683, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void AddNextClient(StoreClient storeClient)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AddNextClient_Public_Void_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301803, RefRangeEnd = 301804, XrefRangeStart = 301684, XrefRangeEnd = 301803, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CheckRentDay()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckRentDay_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301844, RefRangeEnd = 301845, XrefRangeStart = 301804, XrefRangeEnd = 301844, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleSkipClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleSkipClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301884, RefRangeEnd = 301885, XrefRangeStart = 301845, XrefRangeEnd = 301884, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleServiceClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleServiceClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 301885, XrefRangeEnd = 301899, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ComputeInspectionCooldownRange(out int min, out int max)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)Unsafe.AsPointer(ref min);
		*(void**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = Unsafe.AsPointer(ref max);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ComputeInspectionCooldownRange_Private_Void_byref_Int32_byref_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 301919, RefRangeEnd = 301921, XrefRangeStart = 301899, XrefRangeEnd = 301919, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ResetInspection()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ResetInspection_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301929, RefRangeEnd = 301930, XrefRangeStart = 301921, XrefRangeEnd = 301929, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsInspectionCountdownActive()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsInspectionCountdownActive_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 301966, RefRangeEnd = 301967, XrefRangeStart = 301930, XrefRangeEnd = 301966, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleInspectionClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleInspectionClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 301967, XrefRangeEnd = 301989, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleShowCaseSecurity()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleShowCaseSecurity_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 301989, XrefRangeEnd = 301993, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleDebugClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleDebugClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 301993, XrefRangeEnd = 302010, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleSawyerCrewClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleSawyerCrewClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302130, RefRangeEnd = 302131, XrefRangeStart = 302010, XrefRangeEnd = 302130, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleAlwayClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleAlwayClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 302131, XrefRangeEnd = 302154, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleCharacterLine()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleCharacterLine_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 302154, XrefRangeEnd = 302166, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleImportantScriptedClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleImportantScriptedClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302201, RefRangeEnd = 302202, XrefRangeStart = 302166, XrefRangeEnd = 302201, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleWildBailout()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleWildBailout_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 302202, XrefRangeEnd = 302215, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void UpdateWealthTrend()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_UpdateWealthTrend_Private_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 302215, XrefRangeEnd = 302226, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsWealthTrendingDown()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsWealthTrendingDown_Private_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302457, RefRangeEnd = 302458, XrefRangeStart = 302226, XrefRangeEnd = 302457, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleClientFromIntro()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleClientFromIntro_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302485, RefRangeEnd = 302486, XrefRangeStart = 302458, XrefRangeEnd = 302485, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleClientFromEvent()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleClientFromEvent_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302517, RefRangeEnd = 302518, XrefRangeStart = 302486, XrefRangeEnd = 302517, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void UpdateTracker()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_UpdateTracker_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(24)]
	[CachedScanResults(RefRangeStart = 302544, RefRangeEnd = 302568, XrefRangeStart = 302518, XrefRangeEnd = 302544, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void TrackClient(StoreClient storeClient)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TrackClient_Public_Void_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302606, RefRangeEnd = 302607, XrefRangeStart = 302568, XrefRangeEnd = 302606, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleShowcaseClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleShowcaseClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302735, RefRangeEnd = 302736, XrefRangeStart = 302607, XrefRangeEnd = 302735, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandlePerkClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandlePerkClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302785, RefRangeEnd = 302786, XrefRangeStart = 302736, XrefRangeEnd = 302785, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleNormalClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleNormalClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 302826, RefRangeEnd = 302828, XrefRangeStart = 302786, XrefRangeEnd = 302826, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe int GetNormalCustomerCount()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetNormalCustomerCount_Public_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302845, RefRangeEnd = 302846, XrefRangeStart = 302828, XrefRangeEnd = 302845, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleInformationClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleInformationClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302861, RefRangeEnd = 302862, XrefRangeStart = 302846, XrefRangeEnd = 302861, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleSupplierClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleSupplierClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 302862, XrefRangeEnd = 302877, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleInventorClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleInventorClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302982, RefRangeEnd = 302983, XrefRangeStart = 302877, XrefRangeEnd = 302982, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleMarketClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleMarketClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 302985, RefRangeEnd = 302986, XrefRangeStart = 302983, XrefRangeEnd = 302985, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void DismissClientStack()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_DismissClientStack_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 303016, RefRangeEnd = 303017, XrefRangeStart = 302986, XrefRangeEnd = 303016, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe StoreClient GetRandomNewMinorClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetRandomNewMinorClient_Public_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 303038, RefRangeEnd = 303039, XrefRangeStart = 303017, XrefRangeEnd = 303038, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleMinorClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleMinorClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 303067, RefRangeEnd = 303068, XrefRangeStart = 303039, XrefRangeEnd = 303067, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe StoreClient GetRandomValidWantedClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetRandomValidWantedClient_Public_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
	}

	[CallerCount(0)]
	public unsafe int GetRandomClientSpawnChance()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetRandomClientSpawnChance_Public_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 303243, RefRangeEnd = 303244, XrefRangeStart = 303068, XrefRangeEnd = 303243, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleContentUnlockClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleContentUnlockClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 303272, RefRangeEnd = 303273, XrefRangeStart = 303244, XrefRangeEnd = 303272, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleChemicalFeedbackClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleChemicalFeedbackClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 303385, RefRangeEnd = 303387, XrefRangeStart = 303273, XrefRangeEnd = 303385, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CheckStoryLineGeorgio()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckStoryLineGeorgio_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 303472, RefRangeEnd = 303474, XrefRangeStart = 303387, XrefRangeEnd = 303472, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CheckStoryLineOfficerLun()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckStoryLineOfficerLun_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 303548, RefRangeEnd = 303550, XrefRangeStart = 303474, XrefRangeEnd = 303548, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CheckStoryLineEleanor()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckStoryLineEleanor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(10210)]
	[CachedScanResults(RefRangeStart = 18631, RefRangeEnd = 28841, XrefRangeStart = 18631, XrefRangeEnd = 28841, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CheckStoryLineNellis()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckStoryLineNellis_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(10210)]
	[CachedScanResults(RefRangeStart = 18631, RefRangeEnd = 28841, XrefRangeStart = 18631, XrefRangeEnd = 28841, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CheckStoryLineSerpico()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CheckStoryLineSerpico_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 303550, XrefRangeEnd = 303563, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleBlackmailClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleBlackmailClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 303638, RefRangeEnd = 303639, XrefRangeStart = 303563, XrefRangeEnd = 303638, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleJacksonStorage()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleJacksonStorage_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 303639, XrefRangeEnd = 303660, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe string ComputeClientType()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ComputeClientType_Public_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 303660, XrefRangeEnd = 303675, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsCharityComing()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsCharityComing_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 303682, RefRangeEnd = 303683, XrefRangeStart = 303675, XrefRangeEnd = 303682, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int GetCustomerTotalVisitedCount(string identifier)
	{
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(identifier);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetCustomerTotalVisitedCount_Public_Static_Int32_String_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 303724, RefRangeEnd = 303725, XrefRangeStart = 303683, XrefRangeEnd = 303724, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleExpeditionClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleExpeditionClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 303755, RefRangeEnd = 303757, XrefRangeStart = 303725, XrefRangeEnd = 303755, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void HandleAugClient(bool isEarlyCall = false)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&isEarlyCall);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HandleAugClient_Public_Void_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 303757, XrefRangeEnd = 303835, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void AddAugClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AddAugClient_Private_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 303835, XrefRangeEnd = 303864, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe StoreClient FindEventCustomerAugCandidate()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_FindEventCustomerAugCandidate_Private_StoreClient_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 303864, XrefRangeEnd = 303868, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool _GetRandomValidWantedClient_b__91_0(string client)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(client);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__GetRandomValidWantedClient_b__91_0_Private_Boolean_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	public StoreClientManager(System.IntPtr pointer)
		: base(pointer)
	{
	}
}
