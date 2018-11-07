
class GameItem (
    val id: Long,
    val name: String,
    val bigImage: String = "https://bulma.io/images/placeholders/640x480.png",
    val previewImage: String = "https://bulma.io/images/placeholders/256x256.png"
    ) {}

object GameItem {
    val items = List(
        new GameItem(
            0,
            "Меч судьбы 0"
        ),
        new GameItem(
            1,
            "Топор"
        ),
        new GameItem(
            2,
            "Меч судьбы 123"
        ),
        new GameItem(
            3,
            "Сковородка"
        ),
        new GameItem(
            4,
            "Лучший меч ever"
        ),
        new GameItem(
            5,
            "Ящик"
        )
    )

    val myItems = List(0, 1)
    val myLots = List(2, 3)
    val auction = List(4, 5)
}
