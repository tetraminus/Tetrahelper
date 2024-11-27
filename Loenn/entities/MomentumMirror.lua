local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")


local MomentumMirror = {}

MomentumMirror.name = "TetraHelper/MomentumMirror"
MomentumMirror.depth = 0

MomentumMirror.canResize = {true, true}

local topTexture = "objects/MomentumMirror/MMTop00"
local middleTexture = "objects/MomentumMirror/MMMid00"
local bottomTexture = "objects/MomentumMirror/MMBottom00"

MomentumMirror.placements = {
    
	{
        name = "Momentum mirror (Tetrahelper) Right",
        placementType = "rectangle",
        data = {
            height = 8,
            left = true,
           
        }
    },
    {
        name = "Momentum mirror (Tetrahelper) Left",
        placementType = "rectangle",
        data = {
            height = 8,
            left = false,
           
        }
    },
}   
function MomentumMirror.sprite(room, entity)
    local sprites = {}

    local left = entity.left
    local height = entity.height or 8
    local tileHeight = math.floor(height / 8)
    local offsetX = left and 0 or 8
    local scaleX = left and 1 or -1

 

    for i = 2, tileHeight - 1 do
        local middleSprite = drawableSprite.fromTexture(middleTexture, entity)

        middleSprite:addPosition(offsetX, (i - 1) * 8)
        middleSprite:setScale(scaleX, 1)
        middleSprite:setJustification(0.0, 0.0)

        table.insert(sprites, middleSprite)
    end

    local topSprite = drawableSprite.fromTexture(topTexture, entity)
    local bottomSprite = drawableSprite.fromTexture(bottomTexture, entity)

    topSprite:addPosition(offsetX, 0)
    topSprite:setScale(scaleX, 1)
    topSprite:setJustification(0.0, 0.0)

    bottomSprite:addPosition(offsetX, (tileHeight - 1) * 8)
    bottomSprite:setScale(scaleX, 1)
    bottomSprite:setJustification(0.0, 0.0)

    table.insert(sprites, topSprite)
    table.insert(sprites, bottomSprite)

    return sprites
end

function MomentumMirror.rectangle(room, entity)
	
    return utils.rectangle(entity.x, entity.y, entity.width or 8, entity.height or 8)
end



return MomentumMirror
