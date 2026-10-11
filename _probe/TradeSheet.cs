using System;
using System.Runtime.CompilerServices;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;

namespace Il2Cpp;

public class TradeSheet : Il2CppSystem.Object
{
	private static readonly System.IntPtr NativeMethodInfoPtr_GetGenericMilitaryShop_Public_Static_List_1_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetGenericMedicalShop_Public_Static_List_1_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetGenericTraderShop_Public_Static_List_1_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetFoundryShop_Public_Static_List_1_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetEnergyFarmShop_Public_Static_List_1_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetNutrifruitFarmShop_Public_Static_List_1_GameItem_0;

	private static readonly System.IntPtr NativeMethodInfoPtr_GetFenceShop_Public_Static_List_1_GameItem_0;

	static TradeSheet()
	{
		Il2CppClassPointerStore<TradeSheet>.NativeClassPtr = IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "", "TradeSheet");
		IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<TradeSheet>.NativeClassPtr);
		NativeMethodInfoPtr_GetGenericMilitaryShop_Public_Static_List_1_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<TradeSheet>.NativeClassPtr, 100669767);
		NativeMethodInfoPtr_GetGenericMedicalShop_Public_Static_List_1_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<TradeSheet>.NativeClassPtr, 100669768);
		NativeMethodInfoPtr_GetGenericTraderShop_Public_Static_List_1_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<TradeSheet>.NativeClassPtr, 100669769);
		NativeMethodInfoPtr_GetFoundryShop_Public_Static_List_1_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<TradeSheet>.NativeClassPtr, 100669770);
		NativeMethodInfoPtr_GetEnergyFarmShop_Public_Static_List_1_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<TradeSheet>.NativeClassPtr, 100669771);
		NativeMethodInfoPtr_GetNutrifruitFarmShop_Public_Static_List_1_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<TradeSheet>.NativeClassPtr, 100669772);
		NativeMethodInfoPtr_GetFenceShop_Public_Static_List_1_GameItem_0 = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<TradeSheet>.NativeClassPtr, 100669773);
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 233682, RefRangeEnd = 233683, XrefRangeStart = 233442, XrefRangeEnd = 233682, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static List<GameItem> GetGenericMilitaryShop()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetGenericMilitaryShop_Public_Static_List_1_GameItem_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<GameItem>>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 233797, RefRangeEnd = 233798, XrefRangeStart = 233683, XrefRangeEnd = 233797, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static List<GameItem> GetGenericMedicalShop()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetGenericMedicalShop_Public_Static_List_1_GameItem_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<GameItem>>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 234056, RefRangeEnd = 234057, XrefRangeStart = 233798, XrefRangeEnd = 234056, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static List<GameItem> GetGenericTraderShop()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetGenericTraderShop_Public_Static_List_1_GameItem_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<GameItem>>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 234088, RefRangeEnd = 234089, XrefRangeStart = 234057, XrefRangeEnd = 234088, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static List<GameItem> GetFoundryShop()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetFoundryShop_Public_Static_List_1_GameItem_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<GameItem>>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 234102, RefRangeEnd = 234103, XrefRangeStart = 234089, XrefRangeEnd = 234102, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static List<GameItem> GetEnergyFarmShop()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetEnergyFarmShop_Public_Static_List_1_GameItem_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<GameItem>>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 234141, RefRangeEnd = 234142, XrefRangeStart = 234103, XrefRangeEnd = 234141, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static List<GameItem> GetNutrifruitFarmShop()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetNutrifruitFarmShop_Public_Static_List_1_GameItem_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<GameItem>>(intPtr) : null;
	}

	[CallerCount(1)]
	[CachedScanResults(RefRangeStart = 234159, RefRangeEnd = 234160, XrefRangeStart = 234142, XrefRangeEnd = 234159, MetadataInitTokenRva = 0L, MetadataInitFlagRva = 0L)]
	public unsafe static List<GameItem> GetFenceShop()
	{
		System.IntPtr* ptr = null;
		Unsafe.SkipInit(out System.IntPtr intPtr2);
		System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(NativeMethodInfoPtr_GetFenceShop_Public_Static_List_1_GameItem_0, (System.IntPtr)0, (void**)ptr, ref intPtr2);
		Il2CppException.RaiseExceptionIfNecessary(intPtr2);
		return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<List<GameItem>>(intPtr) : null;
	}

	public TradeSheet(System.IntPtr pointer)
		: base(pointer)
	{
	}
}
