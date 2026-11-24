using System.Globalization;
using ZombieOutbreak.Core;

namespace ZombieOutbreak.Data;

/// <summary>
/// 세계지도에 표시되는 168개국 데이터. 코드에 하드코딩하지 않고 '텍스트 표'로 두면 수정·추가가 쉽습니다 (데이터 주도 설계).
/// 형식:  코드|국가명|인구(백만, 2024 근사)|소득(H/M/L)|특성(hub=허브공항)      # 로 시작하는 줄은 주석
/// </summary>
public static class CountryTable
{
    private const string Raw = """
        # 북미
        USA|미국|335|H|hub
        CAN|캐나다|40|H|hub
        MEX|멕시코|129|M|hub
        # 중미·카리브
        GTM|과테말라|18|L|
        BLZ|벨리즈|0.41|M|
        HND|온두라스|10.6|L|
        SLV|엘살바도르|6.3|M|
        NIC|니카라과|7|L|
        CRI|코스타리카|5.2|M|
        PAN|파나마|4.5|M|hub
        CUB|쿠바|11|M|
        HTI|아이티|11.7|L|
        DOM|도미니카공화국|11.3|M|
        JAM|자메이카|2.8|M|
        TTO|트리니다드토바고|1.5|H|
        BHS|바하마|0.41|H|
        # 남미 북부
        BRA|브라질|216|M|hub
        COL|콜롬비아|52|M|hub
        PER|페루|34|M|
        VEN|베네수엘라|28|L|
        ECU|에콰도르|18|M|
        BOL|볼리비아|12.4|L|
        GUY|가이아나|0.8|M|
        SUR|수리남|0.6|M|
        # 남미 남부
        ARG|아르헨티나|46|M|hub
        CHL|칠레|19.6|H|
        PRY|파라과이|6.9|M|
        URY|우루과이|3.4|H|
        # 북유럽
        GBR|영국|68|H|hub
        SWE|스웨덴|10.5|H|
        DNK|덴마크|5.9|H|
        FIN|핀란드|5.6|H|
        NOR|노르웨이|5.5|H|
        IRL|아일랜드|5.2|H|
        LTU|리투아니아|2.9|H|
        LVA|라트비아|1.9|H|
        EST|에스토니아|1.4|H|
        ISL|아이슬란드|0.38|H|
        # 서유럽
        DEU|독일|84|H|hub
        FRA|프랑스|68|H|hub
        NLD|네덜란드|17.9|H|hub
        BEL|벨기에|11.8|H|
        AUT|오스트리아|9.1|H|
        CHE|스위스|8.8|H|
        LUX|룩셈부르크|0.66|H|
        # 남유럽
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
        CYP|키프로스|1.3|H|
        MNE|몬테네그로|0.62|M|
        # 동유럽
        POL|폴란드|37.6|H|
        UKR|우크라이나|37|M|
        ROU|루마니아|19|M|
        CZE|체코|10.9|H|
        HUN|헝가리|9.6|H|
        BLR|벨라루스|9.2|M|
        BGR|불가리아|6.4|M|
        SVK|슬로바키아|5.4|H|
        MDA|몰도바|2.5|L|
        # 러시아·중앙아시아
        RUS|러시아|144|M|hub
        UZB|우즈베키스탄|36|L|
        KAZ|카자흐스탄|20|M|
        TJK|타지키스탄|10|L|
        AZE|아제르바이잔|10.1|M|
        KGZ|키르기스스탄|7|L|
        TKM|투르크메니스탄|6.5|L|
        GEO|조지아|3.7|M|
        ARM|아르메니아|2.8|M|
        # 중동
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
        # 북아프리카
        EGY|이집트|112|M|hub
        SDN|수단|48|L|
        DZA|알제리|45.6|M|
        MAR|모로코|37.8|M|
        TUN|튀니지|12.4|M|
        LBY|리비아|6.9|M|
        # 서아프리카
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
        # 중앙아프리카
        COD|콩고민주공화국|102|L|
        AGO|앙골라|36.7|L|
        CMR|카메룬|28.6|L|
        TCD|차드|18.3|L|
        COG|콩고공화국|6.1|L|
        CAF|중앙아프리카공화국|5.7|L|
        GAB|가봉|2.4|M|
        GNQ|적도기니|1.7|M|
        # 동아프리카
        ETH|에티오피아|126|L|hub
        TZA|탄자니아|67|L|
        KEN|케냐|55|L|
        UGA|우간다|48.6|L|
        MDG|마다가스카르|30|L|
        SOM|소말리아|18|L|
        RWA|르완다|14|L|
        BDI|부룬디|13|L|
        SSD|남수단|11|L|
        ERI|에리트레아|3.7|L|
        DJI|지부티|1.1|L|
        # 남부 아프리카
        ZAF|남아프리카공화국|60|M|hub
        MOZ|모잠비크|33.9|L|
        MWI|말라위|20.9|L|
        ZMB|잠비아|20.6|L|
        ZWE|짐바브웨|16.7|L|
        NAM|나미비아|2.6|M|
        BWA|보츠와나|2.6|M|
        LSO|레소토|2.3|L|
        SWZ|에스와티니|1.2|L|
        # 남아시아
        IND|인도|1430|M|hub
        PAK|파키스탄|240|L|
        BGD|방글라데시|173|L|
        AFG|아프가니스탄|42|L|
        NPL|네팔|30.9|L|
        LKA|스리랑카|22|M|
        BTN|부탄|0.78|M|
        # 동아시아
        CHN|중국|1410|M|hub
        JPN|일본|124|H|hub
        KOR|대한민국|51.7|H|hub
        PRK|북한|26|L|
        TWN|대만|23.4|H|hub
        MNG|몽골|3.4|M|
        # 동남아시아
        IDN|인도네시아|278|M|
        PHL|필리핀|117|M|
        VNM|베트남|99|M|
        THA|태국|71.8|M|hub
        MMR|미얀마|54|L|
        MYS|말레이시아|34|M|
        KHM|캄보디아|17|L|
        LAO|라오스|7.6|L|
        SGP|싱가포르|5.9|H|hub
        TLS|동티모르|1.4|L|
        BRN|브루나이|0.45|H|
        # 오세아니아
        AUS|호주|26.6|H|hub
        PNG|파푸아뉴기니|10.3|L|
        NZL|뉴질랜드|5.2|H|
        FJI|피지|0.93|M|
        SLB|솔로몬제도|0.74|L|
        VUT|바누아투|0.33|L|
        """;

    /// <summary>표를 한 줄씩 읽어 Country 객체 목록을 만듭니다.</summary>
    public static List<Country> Load()
    {
        var list = new List<Country>();
        foreach (var rawLine in Raw.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var p = line.Split('|');
            var wealth = p[3] switch { "H" => Wealth.High, "M" => Wealth.Mid, _ => Wealth.Low };
            double population = double.Parse(p[2], CultureInfo.InvariantCulture) * 1_000_000;
            list.Add(new Country(p[0], p[1], population, wealth, isHub: p[4].Contains("hub")));
        }
        return list;
    }
}
