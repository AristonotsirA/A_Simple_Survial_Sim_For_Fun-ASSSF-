using System.Globalization;
using ZombieOutbreak.Core;

namespace ZombieOutbreak.Data;

/// <summary>
/// 196개국 데이터 (UN 회원국 193 + 바티칸·팔레스타인·대만). 코드에 하드코딩하지 않고 '텍스트 표'로 두면 수정·추가가 쉽습니다 (데이터 주도 설계).
/// 형식:  # 권역ID  /  코드|국가명|인구(백만, 2024 근사)|소득(H/M/L)|특성(hub=허브공항, isl=섬나라)
/// </summary>
public static class CountryTable
{
    private const string Raw = """
        # NorthAmerica
        USA|미국|335|H|hub
        CAN|캐나다|40|H|hub
        MEX|멕시코|129|M|hub
        # CentralAmerica
        GTM|과테말라|18|L|
        BLZ|벨리즈|0.41|M|
        HND|온두라스|10.6|L|
        SLV|엘살바도르|6.3|M|
        NIC|니카라과|7|L|
        CRI|코스타리카|5.2|M|
        PAN|파나마|4.5|M|hub
        CUB|쿠바|11|M|isl
        HTI|아이티|11.7|L|isl
        DOM|도미니카공화국|11.3|M|isl
        JAM|자메이카|2.8|M|isl
        TTO|트리니다드토바고|1.5|H|isl
        BHS|바하마|0.41|H|isl
        BRB|바베이도스|0.28|H|isl
        LCA|세인트루시아|0.18|M|isl
        GRD|그레나다|0.13|M|isl
        VCT|세인트빈센트그레나딘|0.1|M|isl
        ATG|앤티가바부다|0.09|H|isl
        DMA|도미니카연방|0.07|M|isl
        KNA|세인트키츠네비스|0.05|H|isl
        # SouthAmericaNorth
        BRA|브라질|216|M|hub
        COL|콜롬비아|52|M|hub
        PER|페루|34|M|
        VEN|베네수엘라|28|L|
        ECU|에콰도르|18|M|
        BOL|볼리비아|12.4|L|
        GUY|가이아나|0.8|M|
        SUR|수리남|0.6|M|
        # SouthAmericaSouth
        ARG|아르헨티나|46|M|hub
        CHL|칠레|19.6|H|
        PRY|파라과이|6.9|M|
        URY|우루과이|3.4|H|
        # NorthEurope
        GBR|영국|68|H|hub isl
        SWE|스웨덴|10.5|H|
        DNK|덴마크|5.9|H|
        FIN|핀란드|5.6|H|
        NOR|노르웨이|5.5|H|
        IRL|아일랜드|5.2|H|isl
        LTU|리투아니아|2.9|H|
        LVA|라트비아|1.9|H|
        EST|에스토니아|1.4|H|
        ISL|아이슬란드|0.38|H|isl
        # WestEurope
        DEU|독일|84|H|hub
        FRA|프랑스|68|H|hub
        NLD|네덜란드|17.9|H|hub
        BEL|벨기에|11.8|H|
        AUT|오스트리아|9.1|H|
        CHE|스위스|8.8|H|
        LUX|룩셈부르크|0.66|H|
        AND|안도라|0.08|H|
        LIE|리히텐슈타인|0.04|H|
        MCO|모나코|0.04|H|
        # SouthEurope
        ITA|이탈리아|59|H|hub
        ESP|스페인|48|H|hub
        PRT|포르투갈|10.4|H|
        GRC|그리스|10.4|H|
        SRB|세르비아|6.6|M|
        HRV|크로아티아|3.9|H|
        BIH|보스니아헤르체고비나|3.2|M|
        ALB|알바니아|2.8|M|
        SVN|슬로베니아|2.1|H|
        MKD|북마케도니아|1.8|M|
        CYP|키프로스|1.3|H|isl
        MNE|몬테네그로|0.62|M|
        MLT|몰타|0.53|H|isl
        SMR|산마리노|0.03|H|
        VAT|바티칸|0.0008|H|
        # EastEurope
        POL|폴란드|37.6|H|
        UKR|우크라이나|37|M|
        ROU|루마니아|19|M|
        CZE|체코|10.9|H|
        HUN|헝가리|9.6|H|
        BLR|벨라루스|9.2|M|
        BGR|불가리아|6.4|M|
        SVK|슬로바키아|5.4|H|
        MDA|몰도바|2.5|L|
        # RussiaCentralAsia
        RUS|러시아|144|M|hub
        UZB|우즈베키스탄|36|L|
        KAZ|카자흐스탄|20|M|
        TJK|타지키스탄|10|L|
        AZE|아제르바이잔|10.1|M|
        KGZ|키르기스스탄|7|L|
        TKM|투르크메니스탄|6.5|L|
        GEO|조지아|3.7|M|
        ARM|아르메니아|2.8|M|
        # MiddleEast
        IRN|이란|89|M|
        TUR|튀르키예|85|M|hub
        IRQ|이라크|45|L|
        SAU|사우디아라비아|37|H|hub
        YEM|예멘|34|L|
        SYR|시리아|23|L|
        JOR|요르단|11.3|M|
        ARE|아랍에미리트|10|H|hub
        ISR|이스라엘|9.8|H|
        LBN|레바논|5.4|M|
        PSE|팔레스타인|5.4|L|
        OMN|오만|4.6|H|
        KWT|쿠웨이트|4.3|H|
        QAT|카타르|2.7|H|hub
        BHR|바레인|1.5|H|isl
        # NorthAfrica
        EGY|이집트|112|M|hub
        SDN|수단|48|L|
        DZA|알제리|45.6|M|
        MAR|모로코|37.8|M|
        TUN|튀니지|12.4|M|
        LBY|리비아|6.9|M|
        # WestAfrica
        NGA|나이지리아|224|L|hub
        GHA|가나|34|L|
        CIV|코트디부아르|28.9|L|
        NER|니제르|27|L|
        MLI|말리|23|L|
        BFA|부르키나파소|23|L|
        SEN|세네갈|17.8|L|
        GIN|기니|14|L|
        BEN|베냉|13.7|L|
        TGO|토고|9|L|
        SLE|시에라리온|8.8|L|
        LBR|라이베리아|5.4|L|
        MRT|모리타니|4.9|L|
        GMB|감비아|2.8|L|
        GNB|기니비사우|2.2|L|
        CPV|카보베르데|0.6|M|isl
        # CentralAfrica
        COD|콩고민주공화국|102|L|
        AGO|앙골라|36.7|L|
        CMR|카메룬|28.6|L|
        TCD|차드|18.3|L|
        COG|콩고공화국|6.1|L|
        CAF|중앙아프리카공화국|5.7|L|
        GAB|가봉|2.4|M|
        GNQ|적도기니|1.7|M|
        STP|상투메프린시페|0.23|L|isl
        # EastAfrica
        ETH|에티오피아|126|L|hub
        TZA|탄자니아|67|L|
        KEN|케냐|55|L|
        UGA|우간다|48.6|L|
        MDG|마다가스카르|30|L|isl
        SOM|소말리아|18|L|
        RWA|르완다|14|L|
        BDI|부룬디|13|L|
        SSD|남수단|11|L|
        ERI|에리트레아|3.7|L|
        MUS|모리셔스|1.3|M|isl
        DJI|지부티|1.1|L|
        COM|코모로|0.85|L|isl
        SYC|세이셸|0.1|H|isl
        # SouthernAfrica
        ZAF|남아프리카공화국|60|M|hub
        MOZ|모잠비크|33.9|L|
        MWI|말라위|20.9|L|
        ZMB|잠비아|20.6|L|
        ZWE|짐바브웨|16.7|L|
        NAM|나미비아|2.6|M|
        BWA|보츠와나|2.6|M|
        LSO|레소토|2.3|L|
        SWZ|에스와티니|1.2|L|
        # SouthAsia
        IND|인도|1430|M|hub
        PAK|파키스탄|240|L|
        BGD|방글라데시|173|L|
        AFG|아프가니스탄|42|L|
        NPL|네팔|30.9|L|
        LKA|스리랑카|22|M|isl
        BTN|부탄|0.78|M|
        MDV|몰디브|0.52|M|isl
        # EastAsia
        CHN|중국|1410|M|hub
        JPN|일본|124|H|hub isl
        KOR|대한민국|51.7|H|hub
        PRK|북한|26|L|
        TWN|대만|23.4|H|hub isl
        MNG|몽골|3.4|M|
        # SoutheastAsia
        IDN|인도네시아|278|M|isl
        PHL|필리핀|117|M|isl
        VNM|베트남|99|M|
        THA|태국|71.8|M|hub
        MMR|미얀마|54|L|
        MYS|말레이시아|34|M|
        KHM|캄보디아|17|L|
        LAO|라오스|7.6|L|
        SGP|싱가포르|5.9|H|hub isl
        TLS|동티모르|1.4|L|isl
        BRN|브루나이|0.45|H|
        # Oceania
        AUS|호주|26.6|H|hub isl
        PNG|파푸아뉴기니|10.3|L|isl
        NZL|뉴질랜드|5.2|H|isl
        FJI|피지|0.93|M|isl
        SLB|솔로몬제도|0.74|L|isl
        VUT|바누아투|0.33|L|isl
        WSM|사모아|0.22|M|isl
        KIR|키리바시|0.13|L|isl
        FSM|미크로네시아|0.11|M|isl
        TON|통가|0.1|M|isl
        MHL|마셜제도|0.04|M|isl
        NRU|나우루|0.013|M|isl
        TUV|투발루|0.011|L|isl
        PLW|팔라우|0.018|M|isl
        """;

    /// <summary>표를 한 줄씩 읽어 각 권역에 Country 객체를 채워 넣습니다.</summary>
    public static void Populate(List<Region> regions)
    {
        var byId = regions.ToDictionary(r => r.Id);
        Region? current = null;

        foreach (var rawLine in Raw.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;

            if (line.StartsWith('#'))
            {
                current = byId[Enum.Parse<RegionId>(line[1..].Trim())];
                continue;
            }

            var p = line.Split('|');
            var wealth = p[3] switch { "H" => Wealth.High, "M" => Wealth.Mid, _ => Wealth.Low };
            double population = double.Parse(p[2], CultureInfo.InvariantCulture) * 1_000_000;

            current!.Countries.Add(new Country(p[0], p[1], current, population, wealth,
                isHub: p[4].Contains("hub"), isIsland: p[4].Contains("isl")));
        }
    }
}
