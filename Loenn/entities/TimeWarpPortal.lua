local drawableSprite = require("structs.drawable_sprite")

local TimeWarpPortal = {}

TimeWarpPortal.name = "Tetrahelper/TimeWarpPortal"
TimeWarpPortal.depth = -1999
TimeWarpPortal.placements = {
	{
		name = "time_warp_portal (Tetrahelper)",
		data = {
			Target = "portalTarget",
			Flag = ""
		}
	}
}



function TimeWarpPortal.texture(room, entity)
    return "objects/common/warpportal/frame"
end


return TimeWarpPortal