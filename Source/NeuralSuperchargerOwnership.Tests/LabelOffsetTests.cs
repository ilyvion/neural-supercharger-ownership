using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// Where the owner label is drawn on the map, depending on charge and rotation.
[TestFixture(TestType.MainMenu)]
internal sealed class LabelOffsetTests
{
    [Test]
    public static void ChargedHorizontalSuperchargerUsesTheDefaultPosition() =>
        Expect.IsTrue(
            CompAssignableToPawn_NeuralSupercharger.LabelOffsetY(charged: true, horizontal: true)
                == null
        );

    [Test]
    public static void UnchargedHorizontalSuperchargerMovesTheLabelUp() =>
        Expect.IsTrue(
            CompAssignableToPawn_NeuralSupercharger.LabelOffsetY(charged: false, horizontal: true)
                == -15f
        );

    [Test]
    public static void ChargedVerticalSuperchargerMovesTheLabelDown() =>
        Expect.IsTrue(
            CompAssignableToPawn_NeuralSupercharger.LabelOffsetY(charged: true, horizontal: false)
                == 30f
        );

    [Test]
    public static void UnchargedVerticalSuperchargerMovesTheLabelDown() =>
        Expect.IsTrue(
            CompAssignableToPawn_NeuralSupercharger.LabelOffsetY(charged: false, horizontal: false)
                == 30f
        );
}
