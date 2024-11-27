local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local FlingBooster = {}

FlingBooster.name = "Tetrahelper/FlingBooster"
FlingBooster.depth = 0
FlingBooster.fieldInformation = {
    Direction = {
        options = {
			"R",
			"DR",
			"D",
			"DL",
			"L",
			"UL",
			"U",
			"UR"
			
		},
		editable = false
    },
	Speed = {
	 fieldType = "integer"
	}
}

FlingBooster.placements = {
        name = "FlingBooster (Tetrahelper)",
        data = {
           Direction = "R",
		   Speed = 500
        }		
}


local function getTexture(entity)
		local Up = entity.Up
  
		if Up then
			return "objects/FlingBooster/FlingBoosterUpBoost00"
		else 
			return "objects/FlingBooster/FlingBoosterBoost00"
		end
end


function FlingBooster.sprite(room, entity)
    local texture = getTexture(entity)
	local sprite = drawableSprite.fromTexture(texture, entity)
	local left = entity.Left
	if left then
		sprite:setScale(-1.0, 1.0)
	end
  
    

    return sprite
end


return FlingBooster