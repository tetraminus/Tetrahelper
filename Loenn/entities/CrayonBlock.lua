
local crayonBlock = {}

crayonBlock.name = "Tetrahelper/CrayonBlock"
crayonBlock.depth = -13000

function crayonBlock.placements()
    return {
        name = "crayonBlock",
        data = {
            width = 8,
            height = 8
        }
    }
end



return crayonBlock