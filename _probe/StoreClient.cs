using System;
using System.Runtime.CompilerServices;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;

namespace Il2Cpp;

public class StoreClient : Il2CppSystem.Object
{
	[OriginalName("Assembly-CSharp.dll", "", "AugIdleTell")]
	public enum AugIdleTell
	{
		Default,
		Normal,
		Freeze,
		Seizure
	}

	[OriginalName("Assembly-CSharp.dll", "", "ClientIntent")]
	public enum ClientIntent
	{
		UNDEFINED,
		BUY,
		SELL,
		SELLNBUY,
		INSPECTION,
		DIALOGUE,
		BARTER,
		RENT,
		LOAN_SHARK,
		SPECIAL,
		INFORMATION_DEALER,
		PROCUREMENT_OFFER,
		PROCUREMENT_COLLECT,
		WHOLESALE,
		APPRAISAL_SERVICE,
		GUNSMITH,
		EXPEDITION
	}

	[System.Serializable]
	[ObfuscatedName("StoreClient+<>c")]
	public sealed class __c : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr___9;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__121_0;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__140_0;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__140_1;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__141_0;

		private static readonly System.IntPtr NativeFieldInfoPtr___9__141_1;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__AddBasicDialog_b__121_0_Internal_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__SetVariety_b__140_0_Internal_Void_GameItem_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__SetVariety_b__140_1_Internal_Void_GameItem_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__SetTourist_b__141_0_Internal_Void_GameItem_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__SetTourist_b__141_1_Internal_Void_GameItem_0;

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

		public unsafe static Il2CppSystem.Action __9__121_0
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__121_0, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__121_0, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
			}
		}

		public unsafe static Il2CppSystem.Action<GameItem> __9__140_0
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__140_0, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__140_0, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
			}
		}

		public unsafe static Il2CppSystem.Action<GameItem> __9__140_1
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__140_1, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__140_1, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
			}
		}

		public unsafe static Il2CppSystem.Action<GameItem> __9__141_0
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__141_0, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__141_0, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
			}
		}

		public unsafe static Il2CppSystem.Action<GameItem> __9__141_1
		{
			get
			{
				Unsafe.SkipInit(out System.IntPtr intPtr);
				IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr___9__141_1, (void*)(&intPtr));
				System.IntPtr intPtr2 = intPtr;
				return (intPtr2 != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr2) : null;
			}
			set
			{
				IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr___9__141_1, (void*)IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
			}
		}

		static __c()
		{
			Il2CppClassPointerStore<__c>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "<>c");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c>.NativeClassPtr);
			NativeFieldInfoPtr___9 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9");
			NativeFieldInfoPtr___9__121_0 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__121_0");
			NativeFieldInfoPtr___9__140_0 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__140_0");
			NativeFieldInfoPtr___9__140_1 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__140_1");
			NativeFieldInfoPtr___9__141_0 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__141_0");
			NativeFieldInfoPtr___9__141_1 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c>.NativeClassPtr, "<>9__141_1");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100671600);
			NativeMethodInfoPtr__AddBasicDialog_b__121_0_Internal_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100671601);
			NativeMethodInfoPtr__SetVariety_b__140_0_Internal_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100671602);
			NativeMethodInfoPtr__SetVariety_b__140_1_Internal_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100671603);
			NativeMethodInfoPtr__SetTourist_b__141_0_Internal_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100671604);
			NativeMethodInfoPtr__SetTourist_b__141_1_Internal_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c>.NativeClassPtr, 100671605);
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
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298742, XrefRangeEnd = 298747, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _AddBasicDialog_b__121_0()
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__AddBasicDialog_b__121_0_Internal_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298747, XrefRangeEnd = 298756, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _SetVariety_b__140_0(GameItem item)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__SetVariety_b__140_0_Internal_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298756, XrefRangeEnd = 298766, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _SetVariety_b__140_1(GameItem item)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__SetVariety_b__140_1_Internal_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298766, XrefRangeEnd = 298775, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _SetTourist_b__141_0(GameItem item)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__SetTourist_b__141_0_Internal_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298775, XrefRangeEnd = 298785, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _SetTourist_b__141_1(GameItem item)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__SetTourist_b__141_1_Internal_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		public __c(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	[ObfuscatedName("StoreClient+<>c__DisplayClass150_0")]
	public sealed class __c__DisplayClass150_0 : Il2CppSystem.Object
	{
		private static readonly System.IntPtr NativeFieldInfoPtr___4__this;

		private static readonly System.IntPtr NativeFieldInfoPtr_lastLine;

		private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__CompleteSecClient_b__0_Internal_Void_GameItem_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__CompleteSecClient_b__1_Internal_Void_GameItem_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__CompleteSecClient_b__2_Internal_Void_GameItem_0;

		private static readonly System.IntPtr NativeMethodInfoPtr__CompleteSecClient_b__3_Internal_Void_0;

		public unsafe StoreClient __4__this
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr___4__this);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<StoreClient>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr___4__this)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)storeClient));
			}
		}

		public unsafe Dialogue lastLine
		{
			get
			{
				nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_lastLine);
				System.IntPtr intPtr = *(System.IntPtr*)num;
				return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
			}
			set
			{
				System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
				IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_lastLine)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
			}
		}

		static __c__DisplayClass150_0()
		{
			Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr = IL2CPP.GetIl2CppNestedType(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "<>c__DisplayClass150_0");
			IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr);
			NativeFieldInfoPtr___4__this = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr, "<>4__this");
			NativeFieldInfoPtr_lastLine = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr, "lastLine");
			NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr, 100671606);
			NativeMethodInfoPtr__CompleteSecClient_b__0_Internal_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr, 100671607);
			NativeMethodInfoPtr__CompleteSecClient_b__1_Internal_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr, 100671608);
			NativeMethodInfoPtr__CompleteSecClient_b__2_Internal_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr, 100671609);
			NativeMethodInfoPtr__CompleteSecClient_b__3_Internal_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr, 100671610);
		}

		[CallerCount(1403)]
		[CachedScanResults(RefRangeStart = 1155, RefRangeEnd = 2558, XrefRangeStart = 1155, XrefRangeEnd = 2558, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe __c__DisplayClass150_0()
			: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<__c__DisplayClass150_0>.NativeClassPtr))
		{
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298785, XrefRangeEnd = 298801, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _CompleteSecClient_b__0(GameItem item)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__CompleteSecClient_b__0_Internal_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298801, XrefRangeEnd = 298817, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _CompleteSecClient_b__1(GameItem item)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__CompleteSecClient_b__1_Internal_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298817, XrefRangeEnd = 298833, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _CompleteSecClient_b__2(GameItem item)
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__CompleteSecClient_b__2_Internal_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298833, XrefRangeEnd = 298859, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		public unsafe void _CompleteSecClient_b__3()
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__CompleteSecClient_b__3_Internal_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}

		public __c__DisplayClass150_0(System.IntPtr pointer)
			: base(pointer)
		{
		}
	}

	private static readonly System.IntPtr NativeFieldInfoPtr_FACTION_LOWER;

	private static readonly System.IntPtr NativeFieldInfoPtr_FACTION_UPPER;

	private static readonly System.IntPtr NativeFieldInfoPtr_FACTION_MIDDLE;

	private static readonly System.IntPtr NativeFieldInfoPtr_FACTION_SECURITY;

	private static readonly System.IntPtr NativeFieldInfoPtr_FACTION_CRIME;

	private static readonly System.IntPtr NativeFieldInfoPtr_FACTION_TOURIST;

	private static readonly System.IntPtr NativeFieldInfoPtr_displayName;

	private static readonly System.IntPtr NativeFieldInfoPtr_identifier;

	private static readonly System.IntPtr NativeFieldInfoPtr_spriteName;

	private static readonly System.IntPtr NativeFieldInfoPtr_possibleSprites;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientCash;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientBudget;

	private static readonly System.IntPtr NativeFieldInfoPtr_useClientBudget;

	private static readonly System.IntPtr NativeFieldInfoPtr_buyPriceModifier;

	private static readonly System.IntPtr NativeFieldInfoPtr_sellPriceModifier;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientBuyingTagList;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientBuyingIdList;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientBlackTagList;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientBlackIdList;

	private static readonly System.IntPtr NativeFieldInfoPtr_dismissable;

	private static readonly System.IntPtr NativeFieldInfoPtr_arrestable;

	private static readonly System.IntPtr NativeFieldInfoPtr_nonDismissableArrestable;

	private static readonly System.IntPtr NativeFieldInfoPtr_nonShootable;

	private static readonly System.IntPtr NativeFieldInfoPtr_mainDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_isMainDialogueStarted;

	private static readonly System.IntPtr NativeFieldInfoPtr_acceptDealDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_acceptDealDialoguePlayed;

	private static readonly System.IntPtr NativeFieldInfoPtr_acceptDealDialogueLastLine;

	private static readonly System.IntPtr NativeFieldInfoPtr_placedWrongItemWhenSellingToDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_placeRightItemWhenSellingToDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_allDoneDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_repeatDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_interogationDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_glasseDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_customOnArrestDialogue;

	private static readonly System.IntPtr NativeFieldInfoPtr_isIntroduced;

	private static readonly System.IntPtr NativeFieldInfoPtr_barterOffer;

	private static readonly System.IntPtr NativeFieldInfoPtr_procurementData;

	private static readonly System.IntPtr NativeFieldInfoPtr_isBusinessComplete;

	private static readonly System.IntPtr NativeFieldInfoPtr_serviceCost;

	private static readonly System.IntPtr NativeFieldInfoPtr_isOfferWholesale;

	private static readonly System.IntPtr NativeFieldInfoPtr_wholesaleDiscount;

	private static readonly System.IntPtr NativeFieldInfoPtr_isRegularSellDisable;

	private static readonly System.IntPtr NativeFieldInfoPtr_isRequireVariety;

	private static readonly System.IntPtr NativeFieldInfoPtr_varietyTrackingList;

	private static readonly System.IntPtr NativeFieldInfoPtr_cooldown;

	private static readonly System.IntPtr NativeFieldInfoPtr_canClientExposeFeature;

	private static readonly System.IntPtr NativeFieldInfoPtr_onItemAddedToWeighted;

	private static readonly System.IntPtr NativeFieldInfoPtr_onItemAddedToClient;

	private static readonly System.IntPtr NativeFieldInfoPtr_OnItemAddedToShowcase;

	private static readonly System.IntPtr NativeFieldInfoPtr_onItemRemovedToWeighted;

	private static readonly System.IntPtr NativeFieldInfoPtr_onItemAddedToCounters;

	private static readonly System.IntPtr NativeFieldInfoPtr_onItemRemovedToCounters;

	private static readonly System.IntPtr NativeFieldInfoPtr_onItemSold;

	private static readonly System.IntPtr NativeFieldInfoPtr_onItemBought;

	private static readonly System.IntPtr NativeFieldInfoPtr_onDealAccepted;

	private static readonly System.IntPtr NativeFieldInfoPtr_willAcceptItemPreference;

	private static readonly System.IntPtr NativeFieldInfoPtr_OnArrival;

	private static readonly System.IntPtr NativeFieldInfoPtr_OnDismiss;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientNegociationCheck;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientFaction;

	private static readonly System.IntPtr NativeFieldInfoPtr_bubbleColor;

	private static readonly System.IntPtr NativeFieldInfoPtr_tag;

	private static readonly System.IntPtr NativeFieldInfoPtr_isSecurity;

	private static readonly System.IntPtr NativeFieldInfoPtr_isSentenceOfficer;

	private static readonly System.IntPtr NativeFieldInfoPtr_isWanted;

	private static readonly System.IntPtr NativeFieldInfoPtr_wantedPosterId;

	private static readonly System.IntPtr NativeFieldInfoPtr_realName;

	private static readonly System.IntPtr NativeFieldInfoPtr_reportReward;

	private static readonly System.IntPtr NativeFieldInfoPtr_reputationPenalty;

	private static readonly System.IntPtr NativeFieldInfoPtr_isStealthWanted;

	private static readonly System.IntPtr NativeFieldInfoPtr_onCaptureAction;

	private static readonly System.IntPtr NativeFieldInfoPtr_onReportAction;

	private static readonly System.IntPtr NativeFieldInfoPtr_budgetConversionRate;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientImpatience;

	private static readonly System.IntPtr NativeFieldInfoPtr_reported;

	private static readonly System.IntPtr NativeFieldInfoPtr_isMultiBuyDisabled;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientItemFeatureBuying;

	private static readonly System.IntPtr NativeFieldInfoPtr_acceptRetailMarkup;

	private static readonly System.IntPtr NativeFieldInfoPtr_perfumeInfluence;

	private static readonly System.IntPtr NativeFieldInfoPtr_isNoContributeToEvidence;

	private static readonly System.IntPtr NativeFieldInfoPtr_tradeReputationMultiplier;

	private static readonly System.IntPtr NativeFieldInfoPtr_sellTradeRepMultiplierOverride;

	private static readonly System.IntPtr NativeFieldInfoPtr_buyTradeRepMultiplierOverride;

	private static readonly System.IntPtr NativeFieldInfoPtr_noAcceptingContraband;

	private static readonly System.IntPtr NativeFieldInfoPtr_AcceptingContrabandOverride;

	private static readonly System.IntPtr NativeFieldInfoPtr_IsPersonalConsumption;

	private static readonly System.IntPtr NativeFieldInfoPtr_IsClientAddict;

	private static readonly System.IntPtr NativeFieldInfoPtr_CanReturn;

	private static readonly System.IntPtr NativeFieldInfoPtr_CanReturnAsBuyer;

	private static readonly System.IntPtr NativeFieldInfoPtr_CanExposeChem;

	private static readonly System.IntPtr NativeFieldInfoPtr_alwaysFeedback;

	private static readonly System.IntPtr NativeFieldInfoPtr_acceptedChemicalProductGrade;

	private static readonly System.IntPtr NativeFieldInfoPtr_hirelingData;

	private static readonly System.IntPtr NativeFieldInfoPtr_isLazyInspector;

	private static readonly System.IntPtr NativeFieldInfoPtr_isAug;

	private static readonly System.IntPtr NativeFieldInfoPtr_augTier;

	private static readonly System.IntPtr NativeFieldInfoPtr_isAugScanned;

	private static readonly System.IntPtr NativeFieldInfoPtr_augIdleTell;

	private static readonly System.IntPtr NativeFieldInfoPtr_attractedItemId;

	private static readonly System.IntPtr NativeFieldInfoPtr_attractedType;

	private static readonly System.IntPtr NativeFieldInfoPtr_eventSourceId;

	private static readonly System.IntPtr NativeFieldInfoPtr_alwaysRejectBadBargain;

	private static readonly System.IntPtr NativeFieldInfoPtr_isBargainAvailable;

	private static readonly System.IntPtr NativeFieldInfoPtr_clientIntent;

	private static readonly System.IntPtr NativeFieldInfoPtr_lastPostDealGraft;

	private static readonly System.IntPtr NativeMethodInfoPtr_CompleteBusiness_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_get_willAcceptItem_Public_get_Func_2_GameItem_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_set_willAcceptItem_Public_set_Void_Func_2_GameItem_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetTradeRepMultiplier_Public_Double_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ClearTradeRepMultiplierOverrides_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__ctor_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsClientRefusingItem_Public_Boolean_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsRefusingLuxuryItem_Public_Boolean_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsClientBuyingThisItem_Public_Boolean_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CompleteClientCreation_Public_Void_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetScavNoAcceptContrabandChance_Public_Static_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AddBasicDialog_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnDealAccepted_Public_Void_Dialogue_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_TrackItemVariety_Public_Void_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsItemInVarietyList_Public_Boolean_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_HasBudgetLeftToBuy_Public_Boolean_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CanClientExposeAnyFeature_Public_Boolean_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CanClientExposeThisFeature_Public_Boolean_ItemFeature_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnItemPlaced_Public_Void_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ClientExposeFeature_Public_Void_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AddBasicCheck_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetSprite_Public_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnIntroduced_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetClientBudget_Public_Void_Int32_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnItemSold_Public_Void_GameItem_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OnItemBought_Public_Void_GameItem_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetVariety_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetTourist_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetBudget_Public_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_OverrideBudget_Public_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ModBudget_Public_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetBudget_Public_Void_Int32_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetBudget_Public_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_ApplyBudgetModifier_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetWholesale_Public_Void_Int32_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetWanted_Public_Void_Int32_String_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CompleteSecClient_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsWanted_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsSellingContraband_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsSellingStolen_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsClientBuying_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsClientSelling_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_AddTag_Public_Void_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsTag_Public_Boolean_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_SetBarterOffer_Public_Void_BarterOffer_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_StartMainDialogue_Public_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_CanReport_Public_Boolean_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_IsFaction_Public_Boolean_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__get_willAcceptItem_b__59_0_Private_Boolean_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__IsClientRefusingItem_b__116_0_Private_Boolean_String_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__CompleteClientCreation_b__119_0_Private_Void_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__CompleteClientCreation_b__119_1_Private_Void_0;

	private static readonly System.IntPtr NativeMethodInfoPtr__CompleteClientCreation_b__119_2_Private_Void_0;

	public unsafe static string FACTION_LOWER
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_FACTION_LOWER, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_FACTION_LOWER, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static string FACTION_UPPER
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_FACTION_UPPER, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_FACTION_UPPER, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static string FACTION_MIDDLE
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_FACTION_MIDDLE, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_FACTION_MIDDLE, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static string FACTION_SECURITY
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_FACTION_SECURITY, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_FACTION_SECURITY, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static string FACTION_CRIME
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_FACTION_CRIME, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_FACTION_CRIME, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe static string FACTION_TOURIST
	{
		get
		{
			Unsafe.SkipInit(out System.IntPtr intPtr);
			IL2CPP.il2cpp_field_static_get_value(NativeFieldInfoPtr_FACTION_TOURIST, (void*)(&intPtr));
			return IL2CPP.Il2CppStringToManaged(intPtr);
		}
		set
		{
			IL2CPP.il2cpp_field_static_set_value(NativeFieldInfoPtr_FACTION_TOURIST, (void*)IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe string displayName
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_displayName);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_displayName)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe string identifier
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_identifier);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_identifier)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe string spriteName
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_spriteName);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_spriteName)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe List<string> possibleSprites
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_possibleSprites);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_possibleSprites)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe int clientCash
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientCash);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientCash)) = num;
		}
	}

	public unsafe int clientBudget
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBudget);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBudget)) = num;
		}
	}

	public unsafe bool useClientBudget
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_useClientBudget);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_useClientBudget)) = flag;
		}
	}

	public unsafe int buyPriceModifier
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_buyPriceModifier);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_buyPriceModifier)) = num;
		}
	}

	public unsafe int sellPriceModifier
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_sellPriceModifier);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_sellPriceModifier)) = num;
		}
	}

	public unsafe List<string> clientBuyingTagList
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBuyingTagList);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBuyingTagList)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe List<string> clientBuyingIdList
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBuyingIdList);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBuyingIdList)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe List<string> clientBlackTagList
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBlackTagList);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBlackTagList)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe List<string> clientBlackIdList
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBlackIdList);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientBlackIdList)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe bool dismissable
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_dismissable);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_dismissable)) = flag;
		}
	}

	public unsafe bool arrestable
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_arrestable);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_arrestable)) = flag;
		}
	}

	public unsafe bool nonDismissableArrestable
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_nonDismissableArrestable);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_nonDismissableArrestable)) = flag;
		}
	}

	public unsafe bool nonShootable
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_nonShootable);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_nonShootable)) = flag;
		}
	}

	public unsafe Dialogue mainDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_mainDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_mainDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe bool isMainDialogueStarted
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isMainDialogueStarted);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isMainDialogueStarted)) = flag;
		}
	}

	public unsafe Dialogue acceptDealDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptDealDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptDealDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe bool acceptDealDialoguePlayed
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptDealDialoguePlayed);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptDealDialoguePlayed)) = flag;
		}
	}

	public unsafe Dialogue acceptDealDialogueLastLine
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptDealDialogueLastLine);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptDealDialogueLastLine)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe Dialogue placedWrongItemWhenSellingToDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_placedWrongItemWhenSellingToDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_placedWrongItemWhenSellingToDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe Dialogue placeRightItemWhenSellingToDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_placeRightItemWhenSellingToDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_placeRightItemWhenSellingToDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe Dialogue allDoneDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_allDoneDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_allDoneDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe Dialogue repeatDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_repeatDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_repeatDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe Dialogue interogationDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_interogationDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_interogationDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe Dialogue glasseDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_glasseDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_glasseDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe Dialogue customOnArrestDialogue
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_customOnArrestDialogue);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_customOnArrestDialogue)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe bool isIntroduced
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isIntroduced);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isIntroduced)) = flag;
		}
	}

	public unsafe BarterOffer barterOffer
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_barterOffer);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<BarterOffer>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_barterOffer)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)barterOffer));
		}
	}

	public unsafe ProcurementData procurementData
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_procurementData);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<ProcurementData>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_procurementData)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)procurementData));
		}
	}

	public unsafe bool isBusinessComplete
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isBusinessComplete);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isBusinessComplete)) = flag;
		}
	}

	public unsafe int serviceCost
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_serviceCost);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_serviceCost)) = num;
		}
	}

	public unsafe bool isOfferWholesale
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isOfferWholesale);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isOfferWholesale)) = flag;
		}
	}

	public unsafe int wholesaleDiscount
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_wholesaleDiscount);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_wholesaleDiscount)) = num;
		}
	}

	public unsafe bool isRegularSellDisable
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isRegularSellDisable);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isRegularSellDisable)) = flag;
		}
	}

	public unsafe bool isRequireVariety
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isRequireVariety);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isRequireVariety)) = flag;
		}
	}

	public unsafe List<string> varietyTrackingList
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_varietyTrackingList);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_varietyTrackingList)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe int cooldown
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_cooldown);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_cooldown)) = num;
		}
	}

	public unsafe Il2CppSystem.Func<ItemFeature, StoreClient, bool> canClientExposeFeature
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_canClientExposeFeature);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<ItemFeature, StoreClient, bool>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_canClientExposeFeature)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
		}
	}

	public unsafe Il2CppSystem.Action<GameItem> onItemAddedToWeighted
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemAddedToWeighted);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemAddedToWeighted)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action<GameItem> onItemAddedToClient
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemAddedToClient);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemAddedToClient)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action<GameItem> OnItemAddedToShowcase
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_OnItemAddedToShowcase);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_OnItemAddedToShowcase)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action<GameItem> onItemRemovedToWeighted
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemRemovedToWeighted);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemRemovedToWeighted)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action<GameItem> onItemAddedToCounters
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemAddedToCounters);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemAddedToCounters)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action<GameItem> onItemRemovedToCounters
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemRemovedToCounters);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemRemovedToCounters)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action<GameItem, int> onItemSold
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemSold);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem, int>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemSold)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action<GameItem, int> onItemBought
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemBought);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action<GameItem, int>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onItemBought)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action onDealAccepted
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onDealAccepted);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onDealAccepted)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Func<GameItem, bool> willAcceptItemPreference
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_willAcceptItemPreference);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<GameItem, bool>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_willAcceptItemPreference)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)func));
		}
	}

	public unsafe Il2CppSystem.Action OnArrival
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_OnArrival);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_OnArrival)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action OnDismiss
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_OnDismiss);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_OnDismiss)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe List<string> clientNegociationCheck
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientNegociationCheck);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientNegociationCheck)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe string clientFaction
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientFaction);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientFaction)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe string bubbleColor
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bubbleColor);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_bubbleColor)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe List<string> tag
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_tag);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_tag)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe bool isSecurity
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isSecurity);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isSecurity)) = flag;
		}
	}

	public unsafe bool isSentenceOfficer
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isSentenceOfficer);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isSentenceOfficer)) = flag;
		}
	}

	public unsafe bool isWanted
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isWanted);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isWanted)) = flag;
		}
	}

	public unsafe string wantedPosterId
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_wantedPosterId);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_wantedPosterId)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe string realName
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_realName);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_realName)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe int reportReward
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_reportReward);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_reportReward)) = num;
		}
	}

	public unsafe int reputationPenalty
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_reputationPenalty);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_reputationPenalty)) = num;
		}
	}

	public unsafe bool isStealthWanted
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isStealthWanted);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isStealthWanted)) = flag;
		}
	}

	public unsafe Il2CppSystem.Action onCaptureAction
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onCaptureAction);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onCaptureAction)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe Il2CppSystem.Action onReportAction
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onReportAction);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Action>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_onReportAction)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)action));
		}
	}

	public unsafe int budgetConversionRate
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_budgetConversionRate);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_budgetConversionRate)) = num;
		}
	}

	public unsafe int clientImpatience
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientImpatience);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientImpatience)) = num;
		}
	}

	public unsafe bool reported
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_reported);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_reported)) = flag;
		}
	}

	public unsafe bool isMultiBuyDisabled
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isMultiBuyDisabled);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isMultiBuyDisabled)) = flag;
		}
	}

	public unsafe List<string> clientItemFeatureBuying
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientItemFeatureBuying);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<string>>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientItemFeatureBuying)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)list));
		}
	}

	public unsafe bool acceptRetailMarkup
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptRetailMarkup);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptRetailMarkup)) = flag;
		}
	}

	public unsafe int perfumeInfluence
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_perfumeInfluence);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_perfumeInfluence)) = num;
		}
	}

	public unsafe bool isNoContributeToEvidence
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isNoContributeToEvidence);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isNoContributeToEvidence)) = flag;
		}
	}

	public unsafe double tradeReputationMultiplier
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_tradeReputationMultiplier);
			return *(double*)num;
		}
		set
		{
			*(double*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_tradeReputationMultiplier)) = num;
		}
	}

	public unsafe Il2CppSystem.Nullable<double> sellTradeRepMultiplierOverride
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_sellTradeRepMultiplierOverride);
			return new Il2CppSystem.Nullable<double>(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<Il2CppSystem.Nullable<double>>.NativeClassPtr, (System.IntPtr)num));
		}
		set
		{
			// IL cpblk instruction
			Unsafe.CopyBlock((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_sellTradeRepMultiplierOverride), IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)nullable)), IL2CPP.il2cpp_class_value_size(Il2CppClassPointerStore<Il2CppSystem.Nullable<double>>.NativeClassPtr, ref *(uint*)null));
		}
	}

	public unsafe Il2CppSystem.Nullable<double> buyTradeRepMultiplierOverride
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_buyTradeRepMultiplierOverride);
			return new Il2CppSystem.Nullable<double>(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<Il2CppSystem.Nullable<double>>.NativeClassPtr, (System.IntPtr)num));
		}
		set
		{
			// IL cpblk instruction
			Unsafe.CopyBlock((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_buyTradeRepMultiplierOverride), IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)nullable)), IL2CPP.il2cpp_class_value_size(Il2CppClassPointerStore<Il2CppSystem.Nullable<double>>.NativeClassPtr, ref *(uint*)null));
		}
	}

	public unsafe bool noAcceptingContraband
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_noAcceptingContraband);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_noAcceptingContraband)) = flag;
		}
	}

	public unsafe bool AcceptingContrabandOverride
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_AcceptingContrabandOverride);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_AcceptingContrabandOverride)) = flag;
		}
	}

	public unsafe bool IsPersonalConsumption
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_IsPersonalConsumption);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_IsPersonalConsumption)) = flag;
		}
	}

	public unsafe bool IsClientAddict
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_IsClientAddict);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_IsClientAddict)) = flag;
		}
	}

	public unsafe bool CanReturn
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_CanReturn);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_CanReturn)) = flag;
		}
	}

	public unsafe bool CanReturnAsBuyer
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_CanReturnAsBuyer);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_CanReturnAsBuyer)) = flag;
		}
	}

	public unsafe bool CanExposeChem
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_CanExposeChem);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_CanExposeChem)) = flag;
		}
	}

	public unsafe bool alwaysFeedback
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_alwaysFeedback);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_alwaysFeedback)) = flag;
		}
	}

	public unsafe int acceptedChemicalProductGrade
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptedChemicalProductGrade);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_acceptedChemicalProductGrade)) = num;
		}
	}

	public unsafe Hireling hirelingData
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_hirelingData);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Hireling>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_hirelingData)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)hireling));
		}
	}

	public unsafe bool isLazyInspector
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isLazyInspector);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isLazyInspector)) = flag;
		}
	}

	public unsafe bool isAug
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isAug);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isAug)) = flag;
		}
	}

	public unsafe int augTier
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_augTier);
			return *(int*)num;
		}
		set
		{
			*(int*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_augTier)) = num;
		}
	}

	public unsafe bool isAugScanned
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isAugScanned);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isAugScanned)) = flag;
		}
	}

	public unsafe AugIdleTell augIdleTell
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_augIdleTell);
			return *(AugIdleTell*)num;
		}
		set
		{
			*(AugIdleTell*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_augIdleTell)) = augIdleTell;
		}
	}

	public unsafe string attractedItemId
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_attractedItemId);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_attractedItemId)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe string attractedType
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_attractedType);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_attractedType)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe string eventSourceId
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_eventSourceId);
			return IL2CPP.Il2CppStringToManaged(*(System.IntPtr*)num);
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_eventSourceId)), IL2CPP.ManagedStringToIl2Cpp(text));
		}
	}

	public unsafe bool alwaysRejectBadBargain
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_alwaysRejectBadBargain);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_alwaysRejectBadBargain)) = flag;
		}
	}

	public unsafe bool isBargainAvailable
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isBargainAvailable);
			return *(bool*)num;
		}
		set
		{
			*(bool*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_isBargainAvailable)) = flag;
		}
	}

	public unsafe ClientIntent clientIntent
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientIntent);
			return *(ClientIntent*)num;
		}
		set
		{
			*(ClientIntent*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_clientIntent)) = clientIntent;
		}
	}

	public unsafe Dialogue lastPostDealGraft
	{
		get
		{
			nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_lastPostDealGraft);
			System.IntPtr intPtr = *(System.IntPtr*)num;
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Dialogue>(intPtr) : null;
		}
		set
		{
			System.IntPtr num = IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			IL2CPP.il2cpp_gc_wbarrier_set_field(num, (System.IntPtr)((nint)num + (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_lastPostDealGraft)), IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)dialogue));
		}
	}

	public unsafe Il2CppSystem.Func<GameItem, bool> willAcceptItem
	{
		[CallerCount(20)]
		[CachedScanResults(RefRangeStart = 298870, RefRangeEnd = 298890, XrefRangeStart = 298864, XrefRangeEnd = 298870, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		get
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = null;
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_get_willAcceptItem_Public_get_Func_2_GameItem_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
			return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<Il2CppSystem.Func<GameItem, bool>>(intPtr) : null;
		}
		[CallerCount(0)]
		[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 298890, XrefRangeEnd = 298891, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
		set
		{
			IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
			System.IntPtr* ptr = stackalloc System.IntPtr[1];
			*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)value);
			Unsafe.SkipInit(out System.IntPtr intPtr2);
			System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_set_willAcceptItem_Public_set_Void_Func_2_GameItem_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
			Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		}
	}

	static StoreClient()
	{
		Il2CppClassPointerStore<StoreClient>.NativeClassPtr = IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "", "StoreClient");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<StoreClient>.NativeClassPtr);
		NativeFieldInfoPtr_FACTION_LOWER = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "FACTION_LOWER");
		NativeFieldInfoPtr_FACTION_UPPER = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "FACTION_UPPER");
		NativeFieldInfoPtr_FACTION_MIDDLE = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "FACTION_MIDDLE");
		NativeFieldInfoPtr_FACTION_SECURITY = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "FACTION_SECURITY");
		NativeFieldInfoPtr_FACTION_CRIME = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "FACTION_CRIME");
		NativeFieldInfoPtr_FACTION_TOURIST = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "FACTION_TOURIST");
		NativeFieldInfoPtr_displayName = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "displayName");
		NativeFieldInfoPtr_identifier = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "identifier");
		NativeFieldInfoPtr_spriteName = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "spriteName");
		NativeFieldInfoPtr_possibleSprites = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "possibleSprites");
		NativeFieldInfoPtr_clientCash = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientCash");
		NativeFieldInfoPtr_clientBudget = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientBudget");
		NativeFieldInfoPtr_useClientBudget = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "useClientBudget");
		NativeFieldInfoPtr_buyPriceModifier = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "buyPriceModifier");
		NativeFieldInfoPtr_sellPriceModifier = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "sellPriceModifier");
		NativeFieldInfoPtr_clientBuyingTagList = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientBuyingTagList");
		NativeFieldInfoPtr_clientBuyingIdList = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientBuyingIdList");
		NativeFieldInfoPtr_clientBlackTagList = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientBlackTagList");
		NativeFieldInfoPtr_clientBlackIdList = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientBlackIdList");
		NativeFieldInfoPtr_dismissable = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "dismissable");
		NativeFieldInfoPtr_arrestable = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "arrestable");
		NativeFieldInfoPtr_nonDismissableArrestable = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "nonDismissableArrestable");
		NativeFieldInfoPtr_nonShootable = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "nonShootable");
		NativeFieldInfoPtr_mainDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "mainDialogue");
		NativeFieldInfoPtr_isMainDialogueStarted = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isMainDialogueStarted");
		NativeFieldInfoPtr_acceptDealDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "acceptDealDialogue");
		NativeFieldInfoPtr_acceptDealDialoguePlayed = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "acceptDealDialoguePlayed");
		NativeFieldInfoPtr_acceptDealDialogueLastLine = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "acceptDealDialogueLastLine");
		NativeFieldInfoPtr_placedWrongItemWhenSellingToDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "placedWrongItemWhenSellingToDialogue");
		NativeFieldInfoPtr_placeRightItemWhenSellingToDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "placeRightItemWhenSellingToDialogue");
		NativeFieldInfoPtr_allDoneDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "allDoneDialogue");
		NativeFieldInfoPtr_repeatDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "repeatDialogue");
		NativeFieldInfoPtr_interogationDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "interogationDialogue");
		NativeFieldInfoPtr_glasseDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "glasseDialogue");
		NativeFieldInfoPtr_customOnArrestDialogue = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "customOnArrestDialogue");
		NativeFieldInfoPtr_isIntroduced = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isIntroduced");
		NativeFieldInfoPtr_barterOffer = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "barterOffer");
		NativeFieldInfoPtr_procurementData = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "procurementData");
		NativeFieldInfoPtr_isBusinessComplete = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isBusinessComplete");
		NativeFieldInfoPtr_serviceCost = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "serviceCost");
		NativeFieldInfoPtr_isOfferWholesale = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isOfferWholesale");
		NativeFieldInfoPtr_wholesaleDiscount = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "wholesaleDiscount");
		NativeFieldInfoPtr_isRegularSellDisable = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isRegularSellDisable");
		NativeFieldInfoPtr_isRequireVariety = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isRequireVariety");
		NativeFieldInfoPtr_varietyTrackingList = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "varietyTrackingList");
		NativeFieldInfoPtr_cooldown = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "cooldown");
		NativeFieldInfoPtr_canClientExposeFeature = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "canClientExposeFeature");
		NativeFieldInfoPtr_onItemAddedToWeighted = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onItemAddedToWeighted");
		NativeFieldInfoPtr_onItemAddedToClient = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onItemAddedToClient");
		NativeFieldInfoPtr_OnItemAddedToShowcase = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "OnItemAddedToShowcase");
		NativeFieldInfoPtr_onItemRemovedToWeighted = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onItemRemovedToWeighted");
		NativeFieldInfoPtr_onItemAddedToCounters = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onItemAddedToCounters");
		NativeFieldInfoPtr_onItemRemovedToCounters = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onItemRemovedToCounters");
		NativeFieldInfoPtr_onItemSold = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onItemSold");
		NativeFieldInfoPtr_onItemBought = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onItemBought");
		NativeFieldInfoPtr_onDealAccepted = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onDealAccepted");
		NativeFieldInfoPtr_willAcceptItemPreference = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "willAcceptItemPreference");
		NativeFieldInfoPtr_OnArrival = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "OnArrival");
		NativeFieldInfoPtr_OnDismiss = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "OnDismiss");
		NativeFieldInfoPtr_clientNegociationCheck = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientNegociationCheck");
		NativeFieldInfoPtr_clientFaction = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientFaction");
		NativeFieldInfoPtr_bubbleColor = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "bubbleColor");
		NativeFieldInfoPtr_tag = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "tag");
		NativeFieldInfoPtr_isSecurity = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isSecurity");
		NativeFieldInfoPtr_isSentenceOfficer = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isSentenceOfficer");
		NativeFieldInfoPtr_isWanted = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isWanted");
		NativeFieldInfoPtr_wantedPosterId = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "wantedPosterId");
		NativeFieldInfoPtr_realName = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "realName");
		NativeFieldInfoPtr_reportReward = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "reportReward");
		NativeFieldInfoPtr_reputationPenalty = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "reputationPenalty");
		NativeFieldInfoPtr_isStealthWanted = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isStealthWanted");
		NativeFieldInfoPtr_onCaptureAction = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onCaptureAction");
		NativeFieldInfoPtr_onReportAction = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "onReportAction");
		NativeFieldInfoPtr_budgetConversionRate = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "budgetConversionRate");
		NativeFieldInfoPtr_clientImpatience = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientImpatience");
		NativeFieldInfoPtr_reported = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "reported");
		NativeFieldInfoPtr_isMultiBuyDisabled = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isMultiBuyDisabled");
		NativeFieldInfoPtr_clientItemFeatureBuying = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientItemFeatureBuying");
		NativeFieldInfoPtr_acceptRetailMarkup = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "acceptRetailMarkup");
		NativeFieldInfoPtr_perfumeInfluence = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "perfumeInfluence");
		NativeFieldInfoPtr_isNoContributeToEvidence = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isNoContributeToEvidence");
		NativeFieldInfoPtr_tradeReputationMultiplier = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "tradeReputationMultiplier");
		NativeFieldInfoPtr_sellTradeRepMultiplierOverride = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "sellTradeRepMultiplierOverride");
		NativeFieldInfoPtr_buyTradeRepMultiplierOverride = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "buyTradeRepMultiplierOverride");
		NativeFieldInfoPtr_noAcceptingContraband = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "noAcceptingContraband");
		NativeFieldInfoPtr_AcceptingContrabandOverride = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "AcceptingContrabandOverride");
		NativeFieldInfoPtr_IsPersonalConsumption = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "IsPersonalConsumption");
		NativeFieldInfoPtr_IsClientAddict = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "IsClientAddict");
		NativeFieldInfoPtr_CanReturn = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "CanReturn");
		NativeFieldInfoPtr_CanReturnAsBuyer = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "CanReturnAsBuyer");
		NativeFieldInfoPtr_CanExposeChem = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "CanExposeChem");
		NativeFieldInfoPtr_alwaysFeedback = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "alwaysFeedback");
		NativeFieldInfoPtr_acceptedChemicalProductGrade = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "acceptedChemicalProductGrade");
		NativeFieldInfoPtr_hirelingData = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "hirelingData");
		NativeFieldInfoPtr_isLazyInspector = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isLazyInspector");
		NativeFieldInfoPtr_isAug = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isAug");
		NativeFieldInfoPtr_augTier = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "augTier");
		NativeFieldInfoPtr_isAugScanned = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isAugScanned");
		NativeFieldInfoPtr_augIdleTell = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "augIdleTell");
		NativeFieldInfoPtr_attractedItemId = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "attractedItemId");
		NativeFieldInfoPtr_attractedType = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "attractedType");
		NativeFieldInfoPtr_eventSourceId = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "eventSourceId");
		NativeFieldInfoPtr_alwaysRejectBadBargain = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "alwaysRejectBadBargain");
		NativeFieldInfoPtr_isBargainAvailable = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "isBargainAvailable");
		NativeFieldInfoPtr_clientIntent = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "clientIntent");
		NativeFieldInfoPtr_lastPostDealGraft = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, "lastPostDealGraft");
		NativeMethodInfoPtr_CompleteBusiness_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671546);
		NativeMethodInfoPtr_get_willAcceptItem_Public_get_Func_2_GameItem_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671547);
		NativeMethodInfoPtr_set_willAcceptItem_Public_set_Void_Func_2_GameItem_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671548);
		NativeMethodInfoPtr_GetTradeRepMultiplier_Public_Double_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671549);
		NativeMethodInfoPtr_ClearTradeRepMultiplierOverrides_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671550);
		NativeMethodInfoPtr__ctor_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671551);
		NativeMethodInfoPtr_IsClientRefusingItem_Public_Boolean_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671552);
		NativeMethodInfoPtr_IsRefusingLuxuryItem_Public_Boolean_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671553);
		NativeMethodInfoPtr_IsClientBuyingThisItem_Public_Boolean_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671554);
		NativeMethodInfoPtr_CompleteClientCreation_Public_Void_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671555);
		NativeMethodInfoPtr_GetScavNoAcceptContrabandChance_Public_Static_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671556);
		NativeMethodInfoPtr_AddBasicDialog_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671557);
		NativeMethodInfoPtr_OnDealAccepted_Public_Void_Dialogue_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671558);
		NativeMethodInfoPtr_TrackItemVariety_Public_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671559);
		NativeMethodInfoPtr_IsItemInVarietyList_Public_Boolean_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671560);
		NativeMethodInfoPtr_HasBudgetLeftToBuy_Public_Boolean_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671561);
		NativeMethodInfoPtr_CanClientExposeAnyFeature_Public_Boolean_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671562);
		NativeMethodInfoPtr_CanClientExposeThisFeature_Public_Boolean_ItemFeature_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671563);
		NativeMethodInfoPtr_OnItemPlaced_Public_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671564);
		NativeMethodInfoPtr_ClientExposeFeature_Public_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671565);
		NativeMethodInfoPtr_AddBasicCheck_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671566);
		NativeMethodInfoPtr_GetSprite_Public_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671567);
		NativeMethodInfoPtr_OnIntroduced_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671568);
		NativeMethodInfoPtr_SetClientBudget_Public_Void_Int32_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671569);
		NativeMethodInfoPtr_OnItemSold_Public_Void_GameItem_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671570);
		NativeMethodInfoPtr_OnItemBought_Public_Void_GameItem_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671571);
		NativeMethodInfoPtr_SetVariety_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671572);
		NativeMethodInfoPtr_SetTourist_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671573);
		NativeMethodInfoPtr_SetBudget_Public_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671574);
		NativeMethodInfoPtr_OverrideBudget_Public_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671575);
		NativeMethodInfoPtr_ModBudget_Public_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671576);
		NativeMethodInfoPtr_SetBudget_Public_Void_Int32_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671577);
		NativeMethodInfoPtr_GetBudget_Public_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671578);
		NativeMethodInfoPtr_ApplyBudgetModifier_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671579);
		NativeMethodInfoPtr_SetWholesale_Public_Void_Int32_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671580);
		NativeMethodInfoPtr_SetWanted_Public_Void_Int32_String_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671581);
		NativeMethodInfoPtr_CompleteSecClient_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671582);
		NativeMethodInfoPtr_IsWanted_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671583);
		NativeMethodInfoPtr_IsSellingContraband_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671584);
		NativeMethodInfoPtr_IsSellingStolen_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671585);
		NativeMethodInfoPtr_IsClientBuying_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671586);
		NativeMethodInfoPtr_IsClientSelling_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671587);
		NativeMethodInfoPtr_AddTag_Public_Void_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671588);
		NativeMethodInfoPtr_IsTag_Public_Boolean_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671589);
		NativeMethodInfoPtr_SetBarterOffer_Public_Void_BarterOffer_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671590);
		NativeMethodInfoPtr_StartMainDialogue_Public_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671591);
		NativeMethodInfoPtr_CanReport_Public_Boolean_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671592);
		NativeMethodInfoPtr_IsFaction_Public_Boolean_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671593);
		NativeMethodInfoPtr__get_willAcceptItem_b__59_0_Private_Boolean_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671594);
		NativeMethodInfoPtr__IsClientRefusingItem_b__116_0_Private_Boolean_String_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671595);
		NativeMethodInfoPtr__CompleteClientCreation_b__119_0_Private_Void_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671596);
		NativeMethodInfoPtr__CompleteClientCreation_b__119_1_Private_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671597);
		NativeMethodInfoPtr__CompleteClientCreation_b__119_2_Private_Void_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<StoreClient>.NativeClassPtr, 100671598);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 298862, RefRangeEnd = 298864, XrefRangeStart = 298859, XrefRangeEnd = 298862, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CompleteBusiness()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CompleteBusiness_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 298894, RefRangeEnd = 298897, XrefRangeStart = 298891, XrefRangeEnd = 298894, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe double GetTradeRepMultiplier(bool isPlayerSold)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&isPlayerSold);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetTradeRepMultiplier_Public_Double_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(double*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 298897, RefRangeEnd = 298900, XrefRangeStart = 298897, XrefRangeEnd = 298897, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ClearTradeRepMultiplierOverrides()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ClearTradeRepMultiplierOverrides_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(261)]
	[CachedScanResults(RefRangeStart = 298980, RefRangeEnd = 299241, XrefRangeStart = 298900, XrefRangeEnd = 298980, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe StoreClient()
		: this(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<StoreClient>.NativeClassPtr))
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__ctor_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 299289, RefRangeEnd = 299291, XrefRangeStart = 299241, XrefRangeEnd = 299289, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsClientRefusingItem(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsClientRefusingItem_Public_Boolean_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 299305, RefRangeEnd = 299307, XrefRangeStart = 299291, XrefRangeEnd = 299305, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsRefusingLuxuryItem(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsRefusingLuxuryItem_Public_Boolean_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(6)]
	[CachedScanResults(RefRangeStart = 299337, RefRangeEnd = 299343, XrefRangeStart = 299307, XrefRangeEnd = 299337, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsClientBuyingThisItem(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsClientBuyingThisItem_Public_Boolean_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(232)]
	[CachedScanResults(RefRangeStart = 299448, RefRangeEnd = 299680, XrefRangeStart = 299343, XrefRangeEnd = 299448, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CompleteClientCreation(bool IgnoreExposeFunc = false)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&IgnoreExposeFunc);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CompleteClientCreation_Public_Void_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 299680, XrefRangeEnd = 299681, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static int GetScavNoAcceptContrabandChance()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetScavNoAcceptContrabandChance_Public_Static_Int32_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(178)]
	[CachedScanResults(RefRangeStart = 299705, RefRangeEnd = 299883, XrefRangeStart = 299681, XrefRangeEnd = 299705, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void AddBasicDialog()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AddBasicDialog_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(5)]
	[CachedScanResults(RefRangeStart = 299926, RefRangeEnd = 299931, XrefRangeStart = 299883, XrefRangeEnd = 299926, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void OnDealAccepted(Dialogue hostLine = null)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)hostLine);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnDealAccepted_Public_Void_Dialogue_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 299935, RefRangeEnd = 299936, XrefRangeStart = 299931, XrefRangeEnd = 299935, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void TrackItemVariety(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_TrackItemVariety_Public_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(4)]
	[CachedScanResults(RefRangeStart = 299947, RefRangeEnd = 299951, XrefRangeStart = 299936, XrefRangeEnd = 299947, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsItemInVarietyList(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsItemInVarietyList_Public_Boolean_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 299952, RefRangeEnd = 299954, XrefRangeStart = 299951, XrefRangeEnd = 299952, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool HasBudgetLeftToBuy(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_HasBudgetLeftToBuy_Public_Boolean_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 299978, RefRangeEnd = 299980, XrefRangeStart = 299954, XrefRangeEnd = 299978, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool CanClientExposeAnyFeature(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CanClientExposeAnyFeature_Public_Boolean_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 299984, RefRangeEnd = 299986, XrefRangeStart = 299980, XrefRangeEnd = 299984, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool CanClientExposeThisFeature(ItemFeature itemFeature)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)itemFeature);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CanClientExposeThisFeature_Public_Boolean_ItemFeature_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 299986, XrefRangeEnd = 299992, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void OnItemPlaced(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnItemPlaced_Public_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300012, RefRangeEnd = 300013, XrefRangeStart = 299992, XrefRangeEnd = 300012, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ClientExposeFeature(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ClientExposeFeature_Public_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(17)]
	[CachedScanResults(RefRangeStart = 300023, RefRangeEnd = 300040, XrefRangeStart = 300013, XrefRangeEnd = 300023, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void AddBasicCheck()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AddBasicCheck_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 300048, RefRangeEnd = 300051, XrefRangeStart = 300040, XrefRangeEnd = 300048, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe string GetSprite()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetSprite_Public_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return IL2CPP.Il2CppStringToManaged(intPtr);
	}

	[CallerCount(7)]
	[CachedScanResults(RefRangeStart = 300054, RefRangeEnd = 300061, XrefRangeStart = 300051, XrefRangeEnd = 300054, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void OnIntroduced()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnIntroduced_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300065, RefRangeEnd = 300066, XrefRangeStart = 300061, XrefRangeEnd = 300065, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetClientBudget(int amount, int additionalRange)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&amount);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &additionalRange;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetClientBudget_Public_Void_Int32_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300066, RefRangeEnd = 300067, XrefRangeStart = 300066, XrefRangeEnd = 300066, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void OnItemSold(GameItem item, int soldAmount)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &soldAmount;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnItemSold_Public_Void_GameItem_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300067, RefRangeEnd = 300068, XrefRangeStart = 300067, XrefRangeEnd = 300067, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void OnItemBought(GameItem item, int cost)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &cost;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OnItemBought_Public_Void_GameItem_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(13)]
	[CachedScanResults(RefRangeStart = 300115, RefRangeEnd = 300128, XrefRangeStart = 300068, XrefRangeEnd = 300115, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetVariety()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetVariety_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300175, RefRangeEnd = 300176, XrefRangeStart = 300128, XrefRangeEnd = 300175, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetTourist()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetTourist_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(108)]
	[CachedScanResults(RefRangeStart = 300177, RefRangeEnd = 300285, XrefRangeStart = 300176, XrefRangeEnd = 300177, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetBudget(int budget)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&budget);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetBudget_Public_Void_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	public unsafe void OverrideBudget(int budget)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&budget);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_OverrideBudget_Public_Void_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 300285, RefRangeEnd = 300287, XrefRangeStart = 300285, XrefRangeEnd = 300285, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ModBudget(int budgetMod)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&budgetMod);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ModBudget_Public_Void_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 300288, RefRangeEnd = 300290, XrefRangeStart = 300287, XrefRangeEnd = 300288, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetBudget(int budget, int budgetConversion)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[2];
		*ptr = (nint)(&budget);
		*(int**)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = &budgetConversion;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetBudget_Public_Void_Int32_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 53487, RefRangeEnd = 53488, XrefRangeStart = 53487, XrefRangeEnd = 53488, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe int GetBudget()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetBudget_Public_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(int*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(2)]
	[CachedScanResults(RefRangeStart = 300311, RefRangeEnd = 300313, XrefRangeStart = 300290, XrefRangeEnd = 300311, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void ApplyBudgetModifier()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_ApplyBudgetModifier_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(48)]
	[CachedScanResults(RefRangeStart = 300316, RefRangeEnd = 300364, XrefRangeStart = 300313, XrefRangeEnd = 300316, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetWholesale(int percentage)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = (nint)(&percentage);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetWholesale_Public_Void_Int32_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(16)]
	[CachedScanResults(RefRangeStart = 300373, RefRangeEnd = 300389, XrefRangeStart = 300364, XrefRangeEnd = 300373, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetWanted(int reward, string wantedRealName = "", bool isStealthWanted = false)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[3];
		*ptr = (nint)(&reward);
		*(System.IntPtr*)((byte*)ptr + checked((nuint)1u * unchecked((nuint)sizeof(System.IntPtr)))) = IL2CPP.ManagedStringToIl2Cpp(wantedRealName);
		*(bool**)((byte*)ptr + checked((nuint)2u * unchecked((nuint)sizeof(System.IntPtr)))) = &isStealthWanted;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetWanted_Public_Void_Int32_String_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(9)]
	[CachedScanResults(RefRangeStart = 300460, RefRangeEnd = 300469, XrefRangeStart = 300389, XrefRangeEnd = 300460, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void CompleteSecClient()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CompleteSecClient_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300471, RefRangeEnd = 300472, XrefRangeStart = 300469, XrefRangeEnd = 300471, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsWanted()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsWanted_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 300491, RefRangeEnd = 300494, XrefRangeStart = 300472, XrefRangeEnd = 300491, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsSellingContraband()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsSellingContraband_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(3)]
	[CachedScanResults(RefRangeStart = 300513, RefRangeEnd = 300516, XrefRangeStart = 300494, XrefRangeEnd = 300513, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsSellingStolen()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsSellingStolen_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(16)]
	[CachedScanResults(RefRangeStart = 300516, RefRangeEnd = 300532, XrefRangeStart = 300516, XrefRangeEnd = 300516, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsClientBuying()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsClientBuying_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(15)]
	[CachedScanResults(RefRangeStart = 300532, RefRangeEnd = 300547, XrefRangeStart = 300532, XrefRangeEnd = 300532, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsClientSelling()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsClientSelling_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300547, XrefRangeEnd = 300551, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void AddTag(string tag)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(tag);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_AddTag_Public_Void_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(9)]
	[CachedScanResults(RefRangeStart = 300555, RefRangeEnd = 300564, XrefRangeStart = 300551, XrefRangeEnd = 300555, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsTag(string tag)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(tag);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsTag_Public_Boolean_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(7)]
	[CachedScanResults(RefRangeStart = 300566, RefRangeEnd = 300573, XrefRangeStart = 300564, XrefRangeEnd = 300566, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void SetBarterOffer(BarterOffer barterOffer)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)barterOffer);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_SetBarterOffer_Public_Void_BarterOffer_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300576, RefRangeEnd = 300577, XrefRangeStart = 300573, XrefRangeEnd = 300576, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void StartMainDialogue()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_StartMainDialogue_Public_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300588, RefRangeEnd = 300589, XrefRangeStart = 300577, XrefRangeEnd = 300588, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool CanReport()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_CanReport_Public_Boolean_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 300591, RefRangeEnd = 300592, XrefRangeStart = 300589, XrefRangeEnd = 300591, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool IsFaction(string faction)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(faction);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_IsFaction_Public_Boolean_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300592, XrefRangeEnd = 300597, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool _get_willAcceptItem_b__59_0(GameItem gameItem)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)gameItem);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__get_willAcceptItem_b__59_0_Private_Boolean_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300597, XrefRangeEnd = 300601, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe bool _IsClientRefusingItem_b__116_0(string item)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.ManagedStringToIl2Cpp(item);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__IsClientRefusingItem_b__116_0_Private_Boolean_String_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return *(bool*)IL2CPP.il2cpp_object_unbox(intPtr);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300601, XrefRangeEnd = 300620, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void _CompleteClientCreation_b__119_0(GameItem item)
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = stackalloc System.IntPtr[1];
		*ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item);
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__CompleteClientCreation_b__119_0_Private_Void_GameItem_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300620, XrefRangeEnd = 300621, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void _CompleteClientCreation_b__119_1()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__CompleteClientCreation_b__119_1_Private_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	[CallerCount(0)]
	[CachedScanResults(RefRangeStart = 0, RefRangeEnd = 0, XrefRangeStart = 300621, XrefRangeEnd = 300622, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe void _CompleteClientCreation_b__119_2()
	{
		IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr__CompleteClientCreation_b__119_2_Private_Void_0, IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
	}

	public StoreClient(System.IntPtr pointer)
		: base(pointer)
	{
	}
}
