using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace KeepWhatYouPickUp.Data;

// Saved: one item left on the ground in a settlement scene. Field ids are save data: append only.
internal sealed class DroppedItemRecord
{
    [SaveableField(1)] internal string ItemId;
    [SaveableField(2)] internal string ModifierId;
    [SaveableField(3)] internal short Amount;
    [SaveableField(4)] internal float DropDays;
    [SaveableField(5)] internal float Ox; [SaveableField(6)] internal float Oy; [SaveableField(7)] internal float Oz;
    [SaveableField(8)] internal float Sx; [SaveableField(9)] internal float Sy; [SaveableField(10)] internal float Sz;
    [SaveableField(11)] internal float Fx; [SaveableField(12)] internal float Fy; [SaveableField(13)] internal float Fz;
    [SaveableField(14)] internal float Ux; [SaveableField(15)] internal float Uy; [SaveableField(16)] internal float Uz;
    [SaveableField(17)] internal string BannerKey;

    internal MatrixFrame ToFrame() => new(new Mat3(new Vec3(Sx, Sy, Sz), new Vec3(Fx, Fy, Fz), new Vec3(Ux, Uy, Uz)), new Vec3(Ox, Oy, Oz));

    internal static DroppedItemRecord From(string itemId, string modifierId, short amount, string bannerKey, in MatrixFrame f, float dropDays) => new()
    {
        ItemId = itemId, ModifierId = modifierId, Amount = amount, BannerKey = bannerKey, DropDays = dropDays,
        Ox = f.origin.x, Oy = f.origin.y, Oz = f.origin.z,
        Sx = f.rotation.s.x, Sy = f.rotation.s.y, Sz = f.rotation.s.z,
        Fx = f.rotation.f.x, Fy = f.rotation.f.y, Fz = f.rotation.f.z,
        Ux = f.rotation.u.x, Uy = f.rotation.u.y, Uz = f.rotation.u.z,
    };
}