# Stellar Engineer
Your first stop for everything related to modding [The Pegasus Expedition](https://www.thepegasusexpedition.com/)!

## Using Stellar Engineer
At the moment no public release of Stellar Engineer. In the mean time, read a bit about C# programming and [Harmony](https://github.com/pardeike/Harmony).

## Contributing to Stellar Engineer
We alway welcome any contribution to this project! Here's how to set up the project. 
1. Clone this repository
2. Populate the game_dlls folder.
   - For this, go to where the game is installed, and under "The Pegasus Expedition_Data", and then under "Managed" you will have a lot of `.dll` files. If you see a `Assembly-CSharp.dll` then you are in the right folder!
   - Copy every dll to the game_dlls folder.
3. Run `setup.sh` using a bash interpreter. I recommend [Git Bash](https://git-scm.com/downloads)
    - The firs time you run it, it will ask for a path to the game folder, the one that includes the ".exe" folder. Please use an absolute path, and not a relative one!

I also recommend getting [dnspy x64](https://github.com/dnSpy/dnSpy). Also, do also check out the guide on how to [debug Unity Games](https://github.com/dnSpy/dnSpy/wiki/Debugging-Unity-Games) using dnspy. I personally converted my copy of the game to debug build, but the other method should work too. This is all optional, but could be very helpful.

## Troubles? Question?
Don't be afraid to ask in the [Issues Tab](https://github.com/stellar-engineer/StellarEngineer/issues)!