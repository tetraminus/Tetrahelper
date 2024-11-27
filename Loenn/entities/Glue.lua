local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local glue = {}

glue.name = "tetrahelper/Glue"
glue.depth = 1999
glue.canResize = {false, true}
glue.placements = {
    {
        name = "glue_right (Tetrahelper)",
        placementType = "rectangle",
        data = {
            height = 8,
            left = true
        }
    },
    {
        name = "glue_left (Tetrahelper)",
        placementType = "rectangle",
        data = {
            height = 8,
            left = false
        }
    }
}

local fireTopTexture = "objects/glue/fireTop00"
local fireMiddleTexture = "objects/glue/fireMid00"
local fireBottomTexture = "objects/glue/fireBottom00"

local iceTopTexture = "objects/glue/iceTop00"
local iceMiddleTexture = "objects/glue/iceMid00"
local iceBottomTexture = "objects/glue/iceBottom00"

local function getWallTextures(entity)
    if entity.notCoreMode then
        return iceTopTexture, iceMiddleTexture, iceBottomTexture

    else
        return fireTopTexture, fireMiddleTexture, fireBottomTexture
    end
end

function glue.sprite(room, entity)
    local sprites = {}

    local left = entity.left
    local height = entity.height or 8
    local tileHeight = math.floor(height / 8)
    local offsetX = left and 0 or 8
    local scaleX = left and 1 or -1

    local topTexture, middleTexture, bottomTexture = getWallTextures(entity)

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

function glue.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, entity.height or 8)
end

function glue.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.left = not entity.left
        entity.x +  = (entity.left and 8 or -8)
    end

    return horizontal
end

return glue