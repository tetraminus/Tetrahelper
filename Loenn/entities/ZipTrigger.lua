local ziptrigger = {}

ziptrigger.name = "Tetrahelper/ZipTrigger"
ziptrigger.depth = -100
ziptrigger.placements = {
    {
        name = "Zip Trigger (Tetrahelper)",
        data = {
            oneUse = false,
        }
    }
}

function ziptrigger.texture(room, entity)
    return "objects/ziptrigger/idle00"
end

return ziptrigger