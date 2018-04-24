from PIL import Image

def generate_corner(bits, left_bit, diagonal_bit, up_bit, angle):
    left = (bits >> left_bit) & 1
    diagonal = (bits >> diagonal_bit) & 1
    up = (bits >> up_bit) & 1

    bits = (left << 2) | (diagonal << 1) | up

    return [
        img_a,
        img_d,
        img_a,
        img_d,
        img_b,
        img_c,
        img_b,
        img_e
    ][bits].rotate(angle)


def generate(neighbors):

    b = neighbors
    top_left =     generate_corner(b, 4, 7, 6, 0)
    top_right =    generate_corner(b, 6, 5, 3, -90)
    bottom_right = generate_corner(b, 3, 0, 1, 180)
    bottom_left =  generate_corner(b, 1, 2, 4, 90)

    img = Image.new('RGB', (s * 2, s * 2))
    img.paste(top_left,     (0, 0))
    img.paste(top_right,    (s, 0))
    img.paste(bottom_left,  (0, s))
    img.paste(bottom_right, (s, s))
    return img


template = Image.open('Sources/Template.png')

s = 8
img_a = template.crop((s * 0, s * 0, s * 1, s * 1))
img_b = template.crop((s * 1, s * 0, s * 2, s * 1))
img_c = template.crop((s * 2, s * 0, s * 3, s * 1))
img_d = template.crop((s * 0, s * 1, s * 1, s * 2))
img_e = template.crop((s * 1, s * 1, s * 2, s * 2))

for neighbor_bits in range(256):
    img = generate(neighbor_bits)
    img.save('WallTiles/Wall_' + format(neighbor_bits, '08b') + '.png')
