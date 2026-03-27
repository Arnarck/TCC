using UnityEngine;

/////////////////////
/// DOCUMENTATION ///
/////////////////////
/*
Game Instances class.
This class was made to hold script references so we don't need to do 'FindObject<>' all the time whenever we need a script reference.
And as a static class, it will persist across scenes, so we can use it to store data if we need.
 */

public static class GI
{
    public static CardSystem cardSystem;
    public static CardList cardList;
    public static CardNetworkManager networkManager;
}
