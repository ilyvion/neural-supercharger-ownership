using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// The ownership rule behind auto-use (JobGiver_GetNeuralSupercharge), the right-click menu and the
// GetNeuralSupercharge job's fail condition.
[TestFixture(TestType.MainMenu)]
internal sealed class MayUseTests
{
    [Test]
    public static void AnyoneMayUseAnUnownedSupercharger() =>
        Expect.IsTrue(CompAssignableToPawn_NeuralSupercharger.MayUse<string>([], "A"));

    [Test]
    public static void TheOwnerMayUseTheirSupercharger() =>
        Expect.IsTrue(CompAssignableToPawn_NeuralSupercharger.MayUse<string>(["A"], "A"));

    [Test]
    public static void OthersMayNotUseAnOwnedSupercharger() =>
        Expect.IsFalse(CompAssignableToPawn_NeuralSupercharger.MayUse<string>(["A"], "B"));

    [Test]
    public static void NullPawnMayUseAnUnownedSupercharger() =>
        Expect.IsTrue(CompAssignableToPawn_NeuralSupercharger.MayUse<string?>([], null));

    [Test]
    public static void NullPawnMayNotUseAnOwnedSupercharger() =>
        Expect.IsFalse(CompAssignableToPawn_NeuralSupercharger.MayUse<string?>(["A"], null));
}
