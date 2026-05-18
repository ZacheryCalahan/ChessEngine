using static MoveGenerator;

public static class MagicBitboard
{
    public static readonly ulong[] OrthoMask;
    public static readonly ulong[] DiagMask;

    public static readonly ulong[][] OrthoAttacks;
    public static readonly ulong[][] DiagAttacks;

    static MagicBitboard()
    {
        // Initialize the masks
        OrthoMask = new ulong[64];
        DiagMask = new ulong[64];
        for (int i = 0; i < 64; i++)
        {
            OrthoMask[i] = CreateMovementMaskOrtho(i, 0, true);
            DiagMask[i] = CreateMovementMaskDiag(i, 0, true);
        }

        // Initialize the attacks
        OrthoAttacks = new ulong[64][];
        DiagAttacks = new ulong[64][];
        for (int i = 0; i < 64; i++)
        {
            OrthoAttacks[i] = PopulateAttacks(i, true, Magics.OrthoMagics[i], Magics.OrthoShifts[i]);
            DiagAttacks[i] = PopulateAttacks(i, false, Magics.DiagMagics[i], Magics.DiagShifts[i]);
        }
    }

    public static void Init()
    {
        // This TECHNICALLY does nothing in the method, BUT
        // It allows the static constructor to be called whenever.
        // This way we're not generating the tables when we are attempting move search,
        // and instead upon initialization of the engine.
    }

    public static ulong GetSliderAttacks(int square, ulong blockers, bool ortho)
    {
        return ortho ? GetOrthoAttacks(square, blockers) : GetDiagAttacks(square, blockers);
    }

    public static ulong GetOrthoAttacks(int square, ulong blockers)
    {
        ulong key = ((blockers & OrthoMask[square]) * Magics.OrthoMagics[square]) >> Magics.OrthoShifts[square];
        return OrthoAttacks[square][key];
    }

    public static ulong GetDiagAttacks(int square, ulong blockers)
    {
        ulong key = ((blockers & DiagMask[square]) * Magics.DiagMagics[square]) >> Magics.DiagShifts[square];
        return DiagAttacks[square][key];
    }
    
    public enum Dir
    {
        NorthWest,
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West
    }

    public static bool IsDirPositive(int dir) => dir < 4;

    public static ulong DirToRay(int square, Dir dir)
    {
        return dir switch
        {
            Dir.NorthWest => NorthWestRay(square),
            Dir.North     => NorthRay(square),
            Dir.NorthEast => NorthEastRay(square),
            Dir.East      => EastRay(square),
            Dir.SouthEast => SouthEastRay(square),
            Dir.South     => SouthRay(square),
            Dir.SouthWest => SouthWestRay(square),
            Dir.West      => WestRay(square),
            _ => 0
        };
    }

    public static bool IsDirOrtho(int dir) => (dir & 0b1) == 1; // Faster than modulo. Probably.

    static ulong NorthRay(int square) => 0x101010101010100ul << square;
    static ulong EastRay(int square) => ((1ul << (square | 7)) - (1ul << square)) << 1;
    static ulong SouthRay(int square) => 0x0080808080808080ul >> (63 - square);
    static ulong WestRay(int square) => (1UL << square) - (1UL << (square & 56));

    static ulong NorthEastRay(int square)
    {
        ulong mask = 0;
        for (int sq = square; (sq & 7) != 7 && sq < 56;)
        {
            sq += 9;
            mask |= 1UL << sq;
        }
        return mask;
    }

    static ulong SouthEastRay(int square)
    {
        ulong mask = 0;
        for (int sq = square; (sq & 7) != 7 && sq >= 8;)
        {
            sq -= 7;
            mask |= 1UL << sq;
        }
        return mask;

    }

    static ulong SouthWestRay(int square)
    {
        ulong mask = 0;
        for (int sq = square; (sq & 7) != 0 && sq >= 8;)
        {
            sq -= 9;
            mask |= 1UL << sq;
        }
        return mask;
    }

    static ulong NorthWestRay(int square)
    {
        ulong mask = 0;
        for (int sq = square; (sq & 7) != 0 && sq < 56;)
        {
            sq += 7;
            mask |= 1UL << sq;
        }
        return mask;
    }

    // Populate all the possible movements, and optionally each movement with blockers.
    public static ulong CreateMovementMaskOrtho(int square, ulong blockers = 0, bool relevancyMask = false)
    
    {
        ulong mask = 0;

        if (blockers == 0)
        {
            mask |= NorthRay(square);
            mask |= EastRay(square);
            mask |= SouthRay(square);
            mask |= WestRay(square);

            if (relevancyMask)
            {
                ulong relevantMask = 0;
                relevantMask |= Bitboard.MSB(NorthRay(square));
                relevantMask |= Bitboard.MSB(EastRay(square));
                relevantMask |= Bitboard.LSB(SouthRay(square));
                relevantMask |= Bitboard.LSB(WestRay(square));

                mask &= ~relevantMask;
            }

            return mask;
        }

        // Blockers exist, handle them one by one.
        ulong northBlockers = blockers & NorthRay(square);
        ulong eastBlockers = blockers & EastRay(square);
        ulong southBlockers = blockers & SouthRay(square);
        ulong westBlockers = blockers & WestRay(square);
        int northBlockedSquare = Bitboard.LSBToSquare(northBlockers);
        int eastBlockedSquare = Bitboard.LSBToSquare(eastBlockers);
        int southBlockedSquare = Bitboard.MSBToSquare(southBlockers);
        int westBlockedSquare = Bitboard.MSBToSquare(westBlockers);

        if (northBlockedSquare != 64)
            mask |= NorthRay(square) ^ NorthRay(northBlockedSquare);
        else
            mask |= NorthRay(square);

        if (eastBlockedSquare != 64)
            mask |= EastRay(square) ^ EastRay(eastBlockedSquare);
        else
            mask |= EastRay(square);

        if (southBlockedSquare != -1)
            mask |= SouthRay(square) ^ SouthRay(southBlockedSquare);
        else
            mask |= SouthRay(square);

        if (westBlockedSquare != -1)
            mask |= WestRay(square) ^ WestRay(westBlockedSquare);
        else
            mask |= WestRay(square);

        if (relevancyMask)
        {
            ulong relevantMask = 0;
            relevantMask |= Bitboard.MSB(NorthRay(square));
            relevantMask |= Bitboard.MSB(EastRay(square));
            relevantMask |= Bitboard.LSB(SouthRay(square));
            relevantMask |= Bitboard.LSB(WestRay(square));

            mask &= ~relevantMask;
        }

        return mask;

    }

    // Populate all the possible movements, and optionally each movement with blockers.
    public static ulong CreateMovementMaskDiag(int square, ulong blockers = 0, bool relevancyMask = false)
    {
        ulong mask = 0;

        if (blockers == 0)
        {
            mask |= NorthEastRay(square);
            mask |= NorthWestRay(square);
            mask |= SouthWestRay(square);
            mask |= SouthEastRay(square);

            if (relevancyMask)
            {
                ulong relevantMask = 0;
                relevantMask |= Bitboard.MSB(NorthEastRay(square));
                relevantMask |= Bitboard.MSB(NorthWestRay(square));
                relevantMask |= Bitboard.LSB(SouthEastRay(square));
                relevantMask |= Bitboard.LSB(SouthWestRay(square));

                mask &= ~relevantMask;
            }

            return mask;
        }

        // Blockers exist, handle them.
        ulong northEastBlockers = blockers & NorthEastRay(square);
        ulong northWestBlockers = blockers & NorthWestRay(square);
        ulong southEastBlockers = blockers & SouthEastRay(square);
        ulong southWestBlockers = blockers & SouthWestRay(square);
        int northEastBlockedSquare = Bitboard.LSBToSquare(northEastBlockers);
        int northWestBlockedSquare = Bitboard.LSBToSquare(northWestBlockers);
        int southEastBlockedSquare = Bitboard.MSBToSquare(southEastBlockers);
        int southWestBlockedSquare = Bitboard.MSBToSquare(southWestBlockers);

        if (northEastBlockedSquare != 64)
            mask |= NorthEastRay(square) ^ NorthEastRay(northEastBlockedSquare);
        else
            mask |= NorthEastRay(square);

        if (northWestBlockedSquare != 64)
            mask |= NorthWestRay(square) ^ NorthWestRay(northWestBlockedSquare);
        else
            mask |= NorthWestRay(square);

        if (southEastBlockedSquare != -1)
            mask |= SouthEastRay(square) ^ SouthEastRay(southEastBlockedSquare);
        else
            mask |= SouthEastRay(square);

        if (southWestBlockedSquare != -1)
            mask |= SouthWestRay(square) ^ SouthWestRay(southWestBlockedSquare);
        else
            mask |= SouthWestRay(square);

        if (relevancyMask)
        {
            ulong relevantMask = 0;
            relevantMask |= Bitboard.MSB(NorthEastRay(square));
            relevantMask |= Bitboard.MSB(NorthWestRay(square));
            relevantMask |= Bitboard.LSB(SouthEastRay(square));
            relevantMask |= Bitboard.LSB(SouthWestRay(square));

            mask &= ~relevantMask;
        }

        return mask;
    }

    // Populate the magic bitboard table
    static ulong[] PopulateAttacks(int square, bool ortho, ulong magic, int shift)
    {
        int numBits = 64 - shift;
        int lookupSize = 1 << numBits;
        ulong[] table = new ulong[lookupSize];

        ulong movementMask = ortho ? CreateMovementMaskOrtho(square, 0, true) : CreateMovementMaskDiag(square, 0, true);
        ulong[] blockerPatterns = CreateBlockerBitboards(movementMask);

        foreach (ulong pattern in blockerPatterns)
        {
            ulong index = (pattern * magic) >> shift;
            ulong moves = ortho ? CreateMovementMaskOrtho(square, pattern) : CreateMovementMaskDiag(square, pattern);
            table[index] = moves;
        }

        return table;
    }

    static ulong[] CreateBlockerBitboards(ulong moveMask)
    {
        // Pop each LSB in moveMask to determine the positions of each bit
        List<int> moveSquareIndicies = new();
        ulong moveMaskTmp = moveMask;
        while (moveMaskTmp != 0)
        {
            moveSquareIndicies.Add(Bitboard.LSBToSquare(moveMaskTmp)); // Check the lsb
            moveMaskTmp = Bitboard.PopLSB(moveMaskTmp); // Remove after the add
        }

        // Calculate number of bitboards required
        int numBitboards = 1 << moveSquareIndicies.Count;
        ulong[] blockerBitboards = new ulong[numBitboards];

        // Populate and return
        for (int bitboardNum = 0; bitboardNum < numBitboards; bitboardNum++)
        {
            for (int i = 0; i < moveSquareIndicies.Count; i++)
            {
                int bit = (bitboardNum >> i) & 1;
                blockerBitboards[bitboardNum] |= (ulong) bit << moveSquareIndicies[i];
            }
        }

        return blockerBitboards;
    }

    public static class Magics
    {
        // These magics are graciously borrowed from SebLague. Though technically I could compute these myself, I am saving myself the troubles.

        public static readonly int[] OrthoShifts = { 52, 52, 52, 52, 52, 52, 52, 52, 53, 53, 53, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 53, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 52, 54, 53, 53, 53, 53, 54, 53, 52, 53, 54, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 53, 54, 53, 52, 53, 53, 53, 53, 53, 53, 52 };
        public static readonly int[] DiagShifts = { 58, 60, 59, 59, 59, 59, 60, 58, 60, 59, 59, 59, 59, 59, 59, 60, 59, 59, 57, 57, 57, 57, 59, 59, 59, 59, 57, 55, 55, 57, 59, 59, 59, 59, 57, 55, 55, 57, 59, 59, 59, 59, 57, 57, 57, 57, 59, 59, 60, 60, 59, 59, 59, 59, 60, 60, 58, 60, 59, 59, 59, 59, 59, 58 };

        public static readonly ulong[] OrthoMagics = { 468374916371625120, 18428729537625841661, 2531023729696186408, 6093370314119450896, 13830552789156493815, 16134110446239088507, 12677615322350354425, 5404321144167858432, 2111097758984580, 18428720740584907710, 17293734603602787839, 4938760079889530922, 7699325603589095390, 9078693890218258431, 578149610753690728, 9496543503900033792, 1155209038552629657, 9224076274589515780, 1835781998207181184, 509120063316431138, 16634043024132535807, 18446673631917146111, 9623686630121410312, 4648737361302392899, 738591182849868645, 1732936432546219272, 2400543327507449856, 5188164365601475096, 10414575345181196316, 1162492212166789136, 9396848738060210946, 622413200109881612, 7998357718131801918, 7719627227008073923, 16181433497662382080, 18441958655457754079, 1267153596645440, 18446726464209379263, 1214021438038606600, 4650128814733526084, 9656144899867951104, 18444421868610287615, 3695311799139303489, 10597006226145476632, 18436046904206950398, 18446726472933277663, 3458977943764860944, 39125045590687766, 9227453435446560384, 6476955465732358656, 1270314852531077632, 2882448553461416064, 11547238928203796481, 1856618300822323264, 2573991788166144, 4936544992551831040, 13690941749405253631, 15852669863439351807, 18302628748190527413, 12682135449552027479, 13830554446930287982, 18302628782487371519, 7924083509981736956, 4734295326018586370 };
        public static readonly ulong[] DiagMagics = { 16509839532542417919, 14391803910955204223, 1848771770702627364, 347925068195328958, 5189277761285652493, 3750937732777063343, 18429848470517967340, 17870072066711748607, 16715520087474960373, 2459353627279607168, 7061705824611107232, 8089129053103260512, 7414579821471224013, 9520647030890121554, 17142940634164625405, 9187037984654475102, 4933695867036173873, 3035992416931960321, 15052160563071165696, 5876081268917084809, 1153484746652717320, 6365855841584713735, 2463646859659644933, 1453259901463176960, 9808859429721908488, 2829141021535244552, 576619101540319252, 5804014844877275314, 4774660099383771136, 328785038479458864, 2360590652863023124, 569550314443282, 17563974527758635567, 11698101887533589556, 5764964460729992192, 6953579832080335136, 1318441160687747328, 8090717009753444376, 16751172641200572929, 5558033503209157252, 17100156536247493656, 7899286223048400564, 4845135427956654145, 2368485888099072, 2399033289953272320, 6976678428284034058, 3134241565013966284, 8661609558376259840, 17275805361393991679, 15391050065516657151, 11529206229534274423, 9876416274250600448, 16432792402597134585, 11975705497012863580, 11457135419348969979, 9763749252098620046, 16960553411078512574, 15563877356819111679, 14994736884583272463, 9441297368950544394, 14537646123432199168, 9888547162215157388, 18140215579194907366, 18374682062228545019 };
    }
}

