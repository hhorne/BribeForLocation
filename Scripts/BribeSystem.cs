using UnityEngine;
using DaggerfallWorkshop.Game.Entity;
using BribeForLocation;

public class BribeSystem
{
    private readonly PlayerEntity player;

    public BribeSystem(PlayerEntity player)
    {
        this.player = player;
    }

    public int GetBribeAmount()
    {
        int bribeAmount = BribeForLocationMain.Settings.StartingBribeAmount;
        float amountToScaleBy = BribeForLocationMain.Settings.AmountToScaleBy;

        if (BribeForLocationMain.Settings.ScaleByLevel)
        {
            var leveledMultiplier = 1 + (player.Level * amountToScaleBy);
            bribeAmount = Mathf.RoundToInt(bribeAmount * leveledMultiplier);
        }

        return bribeAmount;
    }

    public bool TakeBribe()
    {
        var bribeAmount = GetBribeAmount();

        if (bribeAmount > player.GoldPieces)
            return false;

        player.GoldPieces -= bribeAmount;
        return true;
    }
}
