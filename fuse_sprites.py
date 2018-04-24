from sys import stdout
from glob import glob
import os.path
from PIL import Image

atlas_size = 128
tile_size = 16
tiles_per_row = atlas_size // tile_size
max_number_of_tiles = atlas_size // tile_size * tiles_per_row


def get_tile_coordinates(image_number):
    row = image_number // tiles_per_row
    column = image_number % tiles_per_row
    return (column * tile_size, row * tile_size)

directory = 'WallTiles/'
files = glob('WallTiles/*.png')

# Map image data to tile number
tiles_by_content = {}

name_to_number = {}

# Image objects
tiles = []

atlas = Image.new('RGB', (atlas_size, atlas_size))

number_of_tiles = 0

for filename in files:
    name, ext = os.path.splitext(filename[len(directory):])
    print name

    image = Image.open(filename).convert('RGB')
    key = image.tobytes()

    if key in tiles_by_content:
        # Found a duplicate image
        name_to_number[name] = tiles_by_content[key]
    else:
        name_to_number[name] = number_of_tiles
        tiles_by_content[key] = number_of_tiles

        atlas.paste(image, get_tile_coordinates(number_of_tiles))
        tiles.append(image)
        number_of_tiles += 1

print number_of_tiles, 'unique tiles (max is', max_number_of_tiles, ')'
#atlas.save(target_asset)

with open('Assets/TileNumbers.cs', 'w') as out:
    out.write('public static class TileNumbers {\n')
    out.write('    public static ushort[] numbers = new ushort[] {\n')
    for bits in range(256):
        out.write('        ')
        out.write(str(name_to_number['Wall_{0:08b}'.format(bits)]))
        out.write(',\n')
    out.write('    };\n')
    out.write('}\n')

for number, tile in enumerate(tiles):
    tile.save('Assets/Walls/Wall_%s.png' % number)
