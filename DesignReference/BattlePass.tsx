const assetPathPrefix = "https://www.figma.com/api/mcp/asset/2f32496b-ba5e-4632-873d-a39a7f94c17a";
const img = `${assetPathPrefix}/ab4ba.png`;
const imgGreen = `${assetPathPrefix}/ee01d.png`;
const img1 = `${assetPathPrefix}/e33b9.png`;
const img2 = `${assetPathPrefix}/31dbd.png`;
const img3 = `${assetPathPrefix}/5f4b4.png`;
const img4 = `${assetPathPrefix}/34ef0.png`;
const img5 = `${assetPathPrefix}/01ea2.png`;
const img6 = `${assetPathPrefix}/51d7a.png`;
const img7 = `${assetPathPrefix}/1c51d.png`;
const img8 = `${assetPathPrefix}/78e9c.png`;
const img9 = `${assetPathPrefix}/c5fa6.png`;
const img10 = `${assetPathPrefix}/b5043.png`;
const img11 = `${assetPathPrefix}/bdd8e.png`;
const img12 = `${assetPathPrefix}/c0841.png`;
const img13 = `${assetPathPrefix}/3c7d7.png`;
const img14 = `${assetPathPrefix}/cd792.png`;
const img15 = `${assetPathPrefix}/208db.png`;
const img16 = `${assetPathPrefix}/a78fe.png`;
const img17 = `${assetPathPrefix}/a7534.png`;
const img18 = `${assetPathPrefix}/40f47.png`;
const img19 = `${assetPathPrefix}/c877e.png`;
const img20 = `${assetPathPrefix}/2ccc7.png`;
const img21 = `${assetPathPrefix}/a1284.png`;

function Component({ className }: { className?: string }) {
  return (
    <div className={className || "relative size-[64px]"} data-node-id="167:55" data-name="战令/图标/开通战令">
      <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img} />
    </div>
  );
}

function Green({ className }: { className?: string }) {
  return (
    <div className={className || "h-[24px] relative w-[32px]"} data-node-id="157:60" data-name="战令/领取标记/green">
      <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={imgGreen} />
    </div>
  );
}

function Component1({ className }: { className?: string }) {
  return (
    <div className={className || "relative size-[64px]"} data-node-id="157:31" data-name="战令/图标/普通卡">
      <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img1} />
    </div>
  );
}

export default function Component2() {
  return (
    <div className="bg-[#a7a5a5] relative size-full" data-node-id="40:89" data-name="战令界面">
      <div className="absolute bg-gradient-to-b from-[#a7a5a5] from-[98.736%] h-[803px] left-px to-[#bebdbd] top-[-2px] w-[402px]" data-node-id="59:58" data-name="页面背景" />
      <div className="absolute contents left-[11px] top-[218px]" data-node-id="158:73" data-name="奖励列表 / Lv1-Lv5">
        <div className="absolute bg-gradient-to-b border-2 border-black border-solid from-[#d2d1c6] from-[89%] h-[579px] left-[11px] rounded-[24px] to-[#bebc88] to-[90%] top-[218px] w-[380px]" data-node-id="59:98" data-name="奖励列表底板" />
        <div className="absolute contents left-[20px] top-[243px]" data-node-id="158:13" data-name="Lv1 / 已完成">
          <div className="absolute bg-[#efe5d0] border-2 border-black border-solid h-[92px] left-[195px] rounded-[13px] top-[243px] w-[186px]" data-node-id="59:99" data-name="战令奖励 / Lv1 / 底板" />
          <div className="absolute bg-[#d9d9d9] border-2 border-black border-solid h-[92px] left-[20px] rounded-[13px] top-[243px] w-[168px]" data-node-id="59:100" data-name="免费奖励 / Lv1 / 底板" />
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[32px] py-[var(--space-content-padding,1px)] top-[247px] w-[144px]" data-node-id="158:2" data-name="免费奖励 / Lv1 / 金币 ×500">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:2;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:2;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img2} />
              </div>
              <Green className="absolute h-[24px] left-[56px] top-0 w-[32px]" />
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:2;157:74">
              ​
            </p>
          </div>
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[216px] py-[var(--space-content-padding,1px)] top-[244px] w-[144px]" data-node-id="158:7" data-name="战令奖励 / Lv1 / 普通卡 ×250">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:7;157:71" data-name="图标与标记">
              <Component1 className="absolute left-[40px] size-[64px] top-[8px]" />
              <div className="absolute h-[24px] left-[56px] top-0 w-[32px]" data-node-id="I158:7;157:73" data-name="领取标记">
                <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={img3} />
                <div className="absolute left-[7px] size-[18px] top-px" data-node-id="I158:7;157:73;157:64" data-name="锁">
                  <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img4} />
                </div>
              </div>
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:7;157:74">
              ​
            </p>
          </div>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[104px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[311px] w-[140px]" data-node-id="167:47">
            ×500
          </p>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[288px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[311px] w-[140px]" data-node-id="167:48">
            ×250
          </p>
        </div>
        <div className="absolute contents left-[20px] top-[342px]" data-node-id="158:25" data-name="Lv2 / 已完成">
          <div className="absolute bg-[#efe5d0] border-2 border-black border-solid h-[92px] left-[195px] rounded-[13px] top-[342px] w-[186px]" data-node-id="59:102" data-name="战令奖励 / Lv2 / 底板" />
          <div className="absolute bg-[#d9d9d9] border-2 border-black border-solid h-[92px] left-[20px] rounded-[13px] top-[342px] w-[168px]" data-node-id="59:103" data-name="免费奖励 / Lv2 / 底板" />
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[32px] py-[var(--space-content-padding,1px)] top-[344px] w-[144px]" data-node-id="158:14" data-name="免费奖励 / Lv2 / 钻石 ×10">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:14;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:14;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img5} />
              </div>
              <Green className="absolute h-[24px] left-[56px] top-0 w-[32px]" />
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:14;157:74">
              ​
            </p>
          </div>
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[218px] py-[var(--space-content-padding,1px)] top-[345px] w-[144px]" data-node-id="158:19" data-name="战令奖励 / Lv2 / 稀有卡 ×25">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:19;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:19;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img6} />
              </div>
              <div className="absolute h-[24px] left-[56px] top-0 w-[32px]" data-node-id="I158:19;157:73" data-name="领取标记">
                <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={img3} />
                <div className="absolute left-[7px] size-[18px] top-px" data-node-id="I158:19;157:73;157:64" data-name="锁">
                  <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img4} />
                </div>
              </div>
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:19;157:74">
              ​
            </p>
          </div>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[104px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[410px] w-[140px]" data-node-id="167:49">
            ×10
          </p>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[290px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[410px] w-[140px]" data-node-id="167:50">
            ×25
          </p>
        </div>
        <div className="absolute contents left-[20px] top-[441px]" data-node-id="158:37" data-name="Lv3 / 已完成">
          <div className="absolute bg-[#efe5d0] border-2 border-black border-solid h-[92px] left-[195px] rounded-[13px] top-[441px] w-[186px]" data-node-id="59:105" data-name="战令奖励 / Lv3 / 底板" />
          <div className="absolute bg-[#d9d9d9] border-2 border-black border-solid h-[92px] left-[20px] rounded-[13px] top-[441px] w-[168px]" data-node-id="59:106" data-name="免费奖励 / Lv3 / 底板" />
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[32px] py-[var(--space-content-padding,1px)] top-[452px] w-[144px]" data-node-id="158:26" data-name="免费奖励 / Lv3 / 二星宝箱 ×1">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:26;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:26;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img7} />
              </div>
              <Green className="absolute h-[24px] left-[56px] top-0 w-[32px]" />
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:26;157:74">
              ​
            </p>
          </div>
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[216px] py-[var(--space-content-padding,1px)] top-[450px] w-[144px]" data-node-id="158:31" data-name="战令奖励 / Lv3 / 四星宝箱 ×1">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:31;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:31;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img8} />
              </div>
              <div className="absolute h-[24px] left-[56px] top-0 w-[32px]" data-node-id="I158:31;157:73" data-name="领取标记">
                <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={img3} />
                <div className="absolute left-[7px] size-[18px] top-px" data-node-id="I158:31;157:73;157:64" data-name="锁">
                  <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img4} />
                </div>
              </div>
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:31;157:74">
              ​
            </p>
          </div>
        </div>
        <div className="absolute contents left-[20px] top-[540px]" data-node-id="158:49" data-name="Lv4 / 未完成">
          <div className="absolute bg-[#efe5d0] border-2 border-black border-solid h-[92px] left-[195px] rounded-[13px] top-[540px] w-[186px]" data-node-id="59:108" data-name="战令奖励 / Lv4 / 底板" />
          <div className="absolute bg-[#d9d9d9] border-2 border-black border-solid h-[92px] left-[20px] rounded-[13px] top-[540px] w-[168px]" data-node-id="59:109" data-name="免费奖励 / Lv4 / 底板" />
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[32px] py-[var(--space-content-padding,1px)] top-[540px] w-[144px]" data-node-id="158:38" data-name="免费奖励 / Lv4 / 史诗卡 ×5">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:38;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:38;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img9} />
              </div>
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:38;157:74">
              ​
            </p>
          </div>
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[216px] py-[var(--space-content-padding,1px)] top-[542px] w-[144px]" data-node-id="158:43" data-name="战令奖励 / Lv4 / 钻石 ×25">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:43;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:43;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img5} />
              </div>
              <div className="absolute h-[24px] left-[56px] top-0 w-[32px]" data-node-id="I158:43;157:73" data-name="领取标记">
                <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={img3} />
                <div className="absolute left-[7px] size-[18px] top-px" data-node-id="I158:43;157:73;157:64" data-name="锁">
                  <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img4} />
                </div>
              </div>
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:43;157:74">
              ​
            </p>
          </div>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[104px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[608px] w-[140px]" data-node-id="167:51">
            ×5
          </p>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[288px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[608px] w-[140px]" data-node-id="167:52">
            ×25
          </p>
        </div>
        <div className="absolute contents left-[20px] top-[639px]" data-node-id="158:61" data-name="Lv5 / 未完成">
          <div className="absolute bg-[#efe5d0] border-2 border-black border-solid h-[92px] left-[195px] rounded-[13px] top-[639px] w-[186px]" data-node-id="59:111" data-name="战令奖励 / Lv5 / 底板" />
          <div className="absolute bg-[#d9d9d9] border-2 border-black border-solid h-[92px] left-[20px] rounded-[13px] top-[639px] w-[168px]" data-node-id="59:112" data-name="免费奖励 / Lv5 / 底板" />
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[32px] py-[var(--space-content-padding,1px)] top-[639px] w-[144px]" data-node-id="158:50" data-name="免费奖励 / Lv5 / 传奇卡 ×1">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:50;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:50;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img10} />
              </div>
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:50;157:74">
              ​
            </p>
          </div>
          <div className="absolute content-stretch flex flex-col gap-[var(--space-content-gap,0px)] h-[92px] items-center left-[216px] py-[var(--space-content-padding,1px)] top-[648px] w-[144px]" data-node-id="158:55" data-name="战令奖励 / Lv5 / 五星宝箱 ×1">
            <div className="h-[72px] relative shrink-0 w-[144px]" data-node-id="I158:55;157:71" data-name="图标与标记">
              <div className="absolute left-[40px] size-[64px] top-[8px]" data-node-id="I158:55;157:72" data-name="奖励图标">
                <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img11} />
              </div>
              <div className="absolute h-[24px] left-[56px] top-0 w-[32px]" data-node-id="I158:55;157:73" data-name="领取标记">
                <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={img3} />
                <div className="absolute left-[7px] size-[18px] top-px" data-node-id="I158:55;157:73;157:64" data-name="锁">
                  <img alt="" className="absolute inset-0 max-w-none object-contain pointer-events-none size-full" src={img4} />
                </div>
              </div>
            </div>
            <p className="[word-break:break-word] font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] relative shrink-0 text-[13px] text-[color:var(--color-ink,#242424)] text-center w-[140px]" data-node-id="I158:55;157:74">
              ​
            </p>
          </div>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[104px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[707px] w-[140px]" data-node-id="167:53">
            ×1
          </p>
        </div>
        <div className="absolute contents left-[37px] top-[265px]" data-node-id="158:68" data-name="等级轴与进度 / 从上到下递增">
          <div className="absolute flex items-center justify-center left-[166px] size-[48.991px] top-[265px]" data-node-id="59:101">
            <div className="-rotate-45 flex-none">
              <div className="bg-[var(--color-gold,#fdca15)] border-2 border-black border-solid relative size-[34.642px]" data-name="等级 1 / 已完成·金" />
            </div>
          </div>
          <div className="absolute flex items-center justify-center left-[166px] size-[48.991px] top-[364px]" data-node-id="59:104">
            <div className="-rotate-45 flex-none">
              <div className="bg-[var(--color-gold,#fdca15)] border-2 border-black border-solid relative size-[34.642px]" data-name="等级 2 / 已完成·金" />
            </div>
          </div>
          <div className="absolute flex items-center justify-center left-[166px] size-[48.991px] top-[463px]" data-node-id="59:107">
            <div className="-rotate-45 flex-none">
              <div className="bg-[var(--color-gold,#fdca15)] border-2 border-black border-solid relative size-[34.642px]" data-name="等级 3 / 已完成·金" />
            </div>
          </div>
          <div className="absolute flex items-center justify-center left-[166px] size-[48.991px] top-[562px]" data-node-id="59:110">
            <div className="-rotate-45 flex-none">
              <div className="bg-[var(--color-gray,#5f5f60)] border-2 border-black border-solid relative size-[34.642px]" data-name="等级 4 / 未完成·灰" />
            </div>
          </div>
          <div className="absolute flex items-center justify-center left-[166px] size-[48.991px] top-[661px]" data-node-id="59:113">
            <div className="-rotate-45 flex-none">
              <div className="bg-[var(--color-gray,#5f5f60)] border-2 border-black border-solid relative size-[34.642px]" data-name="等级 5 / 未完成·灰" />
            </div>
          </div>
          <div className="absolute h-[6px] left-[37px] top-[534px] w-[335px]" data-node-id="59:116" style={{ backgroundImage: "linear-gradient(90deg, rgb(210, 209, 198) 0%, rgb(253, 201, 21) 20%, rgb(253, 201, 21) 50%, rgb(253, 201, 21) 80%, rgb(210, 209, 198) 99.519%)" }} data-name="进度位置 / 金色背景" />
          <div className="absolute bg-[#c2e48e] border-2 border-black border-solid h-[31px] left-[163px] rounded-[17.5px] top-[519px] w-[56px]" data-node-id="59:114" data-name="当前进度 / 绿色" />
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[26px] leading-[26px] left-[191px] text-[20px] text-[color:var(--color-ink,#242424)] text-center top-[276px] w-[34px]" data-node-id="158:62">
            1
          </p>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[26px] leading-[26px] left-[191px] text-[20px] text-[color:var(--color-ink,#242424)] text-center top-[375px] w-[34px]" data-node-id="158:63">
            2
          </p>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[26px] leading-[26px] left-[191px] text-[20px] text-[color:var(--color-ink,#242424)] text-center top-[474px] w-[34px]" data-node-id="158:64">
            3
          </p>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[26px] leading-[26px] left-[191px] text-[20px] text-[color:var(--color-white,#f7f7f7)] text-center top-[573px] w-[34px]" data-node-id="158:65">
            4
          </p>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[26px] leading-[26px] left-[191px] text-[20px] text-[color:var(--color-white,#f7f7f7)] text-center top-[672px] w-[34px]" data-node-id="158:66">
            5
          </p>
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[191px] text-[12px] text-[color:var(--color-ink,#242424)] text-center top-[525px] w-[48px]" data-node-id="158:67">
            4 / 10
          </p>
        </div>
        <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[100px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[222px] w-[130px]" data-node-id="158:69">
          免费奖励
        </p>
        <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[290px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[222px] w-[138px]" data-node-id="158:70">
          战令奖励
        </p>
        <div className="absolute contents left-[134px] top-[743px]" data-node-id="158:72" data-name="一键领取 / 已达成的免费奖励">
          <div className="absolute bg-[var(--color-gold,#fdca15)] border-2 border-black border-solid h-[47px] left-[134px] rounded-[12px] top-[743px] w-[112px]" data-node-id="59:118" data-name="一键领取 / 按钮底板" />
          <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[22px] leading-[22px] left-[190px] text-[15px] text-[color:var(--color-ink,#242424)] text-center top-[756px] w-[94px]" data-node-id="158:71">
            一键领取
          </p>
        </div>
      </div>
      <div className="absolute contents left-[11px] top-[58px]" data-node-id="158:79" data-name="战令概览 / 等级、进度与开通入口">
        <div className="absolute bg-[#d2d1c6] border-2 border-black border-solid h-[114px] left-[11px] rounded-[24px] top-[100px] w-[380px]" data-node-id="59:60" />
        <div className="absolute bg-[#fed744] border-2 border-black border-solid h-[17px] left-[66px] rounded-[12px] top-[161px] w-[129px]" data-node-id="59:61" data-name="本级进度 / 金色底槽" />
        <div className="absolute h-[185px] left-[215px] top-[58px] w-[153px]" data-node-id="59:63" data-name="战令旗帜">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={img12} />
        </div>
        <div className="absolute h-[59px] left-[252.5px] top-[157px] w-[86px]" data-node-id="59:95" data-name="开通战令 / 金色底板">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={img13} />
        </div>
        <div className="absolute left-[267px] size-[58px] top-[106px]" data-node-id="59:59" data-name="战令皇冠">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={img14} />
        </div>
        <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[24px] leading-[24px] left-[99px] text-[18px] text-[color:var(--color-ink,#242424)] text-center top-[111px] w-[150px]" data-node-id="158:74">
          战令进度
        </p>
        <div className="absolute bg-[var(--color-green,#c2e48e)] h-[13px] left-[68px] rounded-[6px] top-[163px] w-[48px]" data-node-id="158:76" data-name="本级进度 / 绿色填充" />
        <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[18px] leading-[18px] left-[129px] text-[13px] text-[color:var(--color-ink,#242424)] text-center top-[182px] w-[80px]" data-node-id="158:77">
          4 / 10
        </p>
        <div className="absolute flex items-center justify-center left-[32px] size-[48.991px] top-[137px]" data-node-id="59:119">
          <div className="-rotate-45 flex-none">
            <div className="bg-[#fdc915] border-2 border-black border-solid relative size-[34.642px]" />
          </div>
        </div>
        <p className="-translate-x-1/2 [word-break:break-word] absolute font-['Noto_Sans_SC:Bold'] font-bold h-[26px] leading-[26px] left-[56px] text-[20px] text-[color:var(--color-ink,#242424)] text-center top-[148px] w-[32px]" data-node-id="158:75">
          3
        </p>
        <Component className="absolute left-[273.5px] size-[44px] top-[160px]" />
      </div>
      <div className="absolute contents left-[14px] top-[39px]" data-node-id="158:80" data-name="顶部切换 / 任务·战令·宝箱">
        <div className="absolute bg-[#fbe0b7] border-2 border-black border-solid h-[44px] left-[142px] rounded-[23px] top-[39px] w-[123px]" data-node-id="70:154" data-name="任务" />
        <div className="absolute bg-[#e5cba2] border-2 border-black border-solid h-[44px] left-[14px] rounded-[23px] top-[39px] w-[123px]" data-node-id="70:156" data-name="任务" />
        <div className="absolute bg-[#e5cba2] border-2 border-black border-solid h-[44px] left-[269px] rounded-[23px] top-[39px] w-[123px]" data-node-id="70:158" data-name="任务" />
        <div className="absolute h-[38px] left-[182px] rounded-[10px] top-[42px] w-[42px]" data-node-id="70:155" data-name="任务图标">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none rounded-[10px] size-full" src={img14} />
        </div>
        <div className="absolute h-[38px] left-[54px] rounded-[10px] top-[42px] w-[42px]" data-node-id="70:157" data-name="任务图标">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none rounded-[10px] size-full" src={img15} />
        </div>
        <div className="absolute h-[38px] left-[309px] rounded-[10px] top-[42px] w-[42px]" data-node-id="70:159" data-name="任务图标">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none rounded-[10px] size-full" src={img16} />
        </div>
      </div>
      <div className="absolute contents left-0 top-[805px]" data-node-id="158:81" data-name="底部导航">
        <div className="absolute bg-gradient-to-b from-[#c3c2c2] from-[8.824%] h-[68px] left-0 to-[#565757] top-[806px] w-[402px]" data-node-id="40:90" data-name="底边栏背景板" />
        <div className="absolute bg-[#e2e0e0] border-2 border-black border-solid left-px rounded-[14px] size-[68px] top-[806px]" data-node-id="40:91" data-name="商店底板" />
        <div className="absolute left-px rounded-[14px] size-[68px] top-[807px]" data-node-id="40:92" data-name="商店图标">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none rounded-[14px] size-full" src={img17} />
        </div>
        <div className="absolute bg-[#e2e0e0] border-2 border-black border-solid left-[72px] rounded-[14px] size-[68px] top-[806px]" data-node-id="40:93" data-name="卡组底板" />
        <div className="absolute left-[72px] rounded-[14px] size-[68px] top-[806px]" data-node-id="40:94" data-name="卡组图标">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none rounded-[14px] size-full" src={img18} />
        </div>
        <div className="absolute bg-[#e2e0e0] border-2 border-black border-solid left-[262px] rounded-[14px] size-[68px] top-[806px]" data-node-id="40:95" data-name="部落底板" />
        <div className="absolute left-[262px] rounded-[14px] size-[68px] top-[806px]" data-node-id="40:96" data-name="部落图标">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none rounded-[14px] size-full" src={img19} />
        </div>
        <div className="absolute bg-[#f7f7f7] border border-[#121213] border-solid h-[68px] left-[143px] rounded-[14px] top-[805px] w-[116px]" data-node-id="40:97" data-name="主页底板" />
        <div className="absolute border border-[#121213] border-solid h-[68px] left-[143px] rounded-[14px] top-[806px] w-[116px]" data-node-id="40:98" data-name="主页图标">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none rounded-[14px] size-full" src={img20} />
        </div>
        <div className="absolute bg-[#e2e0e0] border-2 border-black border-solid left-[333px] rounded-[14px] size-[68px] top-[806px]" data-node-id="40:99" data-name="锦标赛底板" />
        <div className="absolute left-[333px] rounded-[14px] size-[68px] top-[806px]" data-node-id="40:100" data-name="锦标赛图标">
          <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none rounded-[14px] size-full" src={img21} />
        </div>
      </div>
    </div>
  );
}

SUPER CRITICAL: The generated React+Tailwind code MUST be converted to match the target project's technology stack and styling system.
1. Analyze the target codebase to identify: technology stack, styling approach, component patterns, and design tokens
2. Convert React syntax to the target framework/library
3. Transform all Tailwind classes to the target styling system while preserving exact visual design
4. Follow the project's existing patterns and conventions
DO NOT install any Tailwind as a dependency unless the user instructs you to do so.


Node ids have been added to the code as data attributes, e.g. `data-node-id="1:2"`.

These styles are contained in the design: 战令 / 奖励名称数量: Font(family: "Noto Sans SC", style: Bold, size: 13, weight: 700, lineHeight: 18, letterSpacing: 0).

Component descriptions: The following components have usage descriptions or documentation links defined in Figma. These descriptions provide important context about the intended usage, best practices, and any constraints for each component. Follow these guidelines when implementing or using these components.

## 战令/奖励内容
**Node ID:** 157:70

奖励内容组件：图标与领取标记可独立替换，名称数量可编辑；隐藏标记表示未来的免费奖励。

## 战令/图标/金币箱
**Node ID:** 157:25

金币箱，独立透明 PNG 素材；不包含数量与状态。

## 战令/领取标记/green
**Node ID:** 157:60

左侧绿色标记

## 战令/图标/普通卡
**Node ID:** 157:31

普通卡，独立透明 PNG 素材；不包含数量与状态。

## 战令/领取标记/locked
**Node ID:** 157:63

金色有锁 · 未开通

## 战令/图标/钻石袋
**Node ID:** 157:28

钻石袋，独立透明 PNG 素材；不包含数量与状态。

## 战令/图标/稀有卡
**Node ID:** 157:34

稀有卡，独立透明 PNG 素材；不包含数量与状态。

## 战令/图标/二星宝箱
**Node ID:** 157:47

二星宝箱，独立透明 PNG 素材；不包含数量与状态。

## 战令/图标/四星宝箱
**Node ID:** 157:53

四星宝箱，独立透明 PNG 素材；不包含数量与状态。

## 战令/图标/史诗卡
**Node ID:** 157:37

史诗卡，独立透明 PNG 素材；不包含数量与状态。

## 战令/图标/传奇卡
**Node ID:** 157:40

传奇卡，独立透明 PNG 素材；不包含数量与状态。

## 战令/图标/五星宝箱
**Node ID:** 157:56

五星宝箱，独立透明 PNG 素材；不包含数量与状态。

## 战令/图标/开通战令
**Node ID:** 167:55

开通战令无文字图标：蓝色皇冠解锁挂锁，独立透明 PNG。

The generated code contains image and SVG URL constants, e.g. const image = `${assetPathPrefix}/<asset file name>`, where assetPathPrefix is declared once at the top of the code and every asset URL interpolates it. These URLs are temporary download sources and expire after 7 days. Download each asset into the project with curl -L, then replace its URL in the generated code with a reference to the downloaded asset. Never use the temporary URLs as runtime image sources.
