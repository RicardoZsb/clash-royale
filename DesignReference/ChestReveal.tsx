const assetPathPrefix = "https://www.figma.com/api/mcp/asset/db4b3919-1d17-4397-9450-b04c1ae56a67";
const imgRectangle33 = `${assetPathPrefix}/f2a30.png`;
const imgRectangle36 = `${assetPathPrefix}/ce39e.png`;

export default function Component() {
  return (
    <div className="bg-[#5a5b5c] relative size-full" data-node-id="40:101" data-name="宝箱领取界面">
      <div className="absolute h-[874px] left-0 top-0 w-[402px]" data-node-id="62:123" style={{ backgroundImage: "url(\"data:image/svg+xml;utf8,<svg viewBox='0 0 402 874' xmlns='http://www.w3.org/2000/svg' preserveAspectRatio='none'><rect x='0' y='0' height='100%' width='100%' fill='url(%23grad)' opacity='1'/><defs><radialGradient id='grad' gradientUnits='userSpaceOnUse' cx='0' cy='0' r='10' gradientTransform='matrix(1.2308e-15 43.7 -20.1 2.6759e-15 201 437)'><stop stop-color='rgba(255,225,77,1)' offset='0'/><stop stop-color='rgba(246,221,120,1)' offset='0.5'/><stop stop-color='rgba(236,216,163,1)' offset='1'/></radialGradient></defs></svg>\")" }} />
      <div className="absolute left-[143px] size-[55px] top-[673px]" data-node-id="63:129">
        <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={imgRectangle33} />
      </div>
      <div className="absolute left-[197px] size-[55px] top-[673px]" data-node-id="63:130">
        <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={imgRectangle33} />
      </div>
      <div className="absolute left-[251px] size-[55px] top-[673px]" data-node-id="63:131">
        <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={imgRectangle33} />
      </div>
      <div className="absolute left-[67px] size-[256px] top-[247px]" data-node-id="63:132">
        <img alt="" className="absolute inset-0 max-w-none object-cover pointer-events-none size-full" src={imgRectangle36} />
      </div>
      <div className="absolute bg-[#383a3c] left-[96px] rounded-[21px] size-[42px] top-[680px]" data-node-id="63:133" />
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

The generated code contains image and SVG URL constants, e.g. const image = `${assetPathPrefix}/<asset file name>`, where assetPathPrefix is declared once at the top of the code and every asset URL interpolates it. These URLs are temporary download sources and expire after 7 days. Download each asset into the project with curl -L, then replace its URL in the generated code with a reference to the downloaded asset. Never use the temporary URLs as runtime image sources.
