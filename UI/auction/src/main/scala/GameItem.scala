
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
            "Меч судьбы 0",
            "https://game-icons.net/icons/lorc/originals/svg/relic-blade.svg",
            "https://game-icons.net/icons/lorc/originals/svg/relic-blade.svg"
        ),
        new GameItem(
            1,
            "Топор",
            "https://game-icons.net/icons/delapouite/originals/svg/axe-in-log.svg",
            "https://game-icons.net/icons/delapouite/originals/svg/axe-in-log.svg"
        ),
        new GameItem(
            2,
            "Меч судьбы 123",
            "https://game-icons.net/icons/lorc/originals/svg/relic-blade.svg",
            "https://game-icons.net/icons/lorc/originals/svg/relic-blade.svg"
        ),
        new GameItem(
            3,
            "Лук",
            "https://game-icons.net/icons/lorc/originals/svg/pocket-bow.svg",
            "https://game-icons.net/icons/lorc/originals/svg/pocket-bow.svg"
        ),
        new GameItem(
            4,
            "Лучший меч ever",
            "https://game-icons.net/icons/lorc/originals/svg/broadsword.svg",
            "https://game-icons.net/icons/lorc/originals/svg/broadsword.svg"
        ),
        new GameItem(
            5,
            "Ящик",
            "https://game-icons.net/icons/delapouite/originals/svg/wooden-crate.svg",
            "https://game-icons.net/icons/delapouite/originals/svg/wooden-crate.svg"
        )
    )

    val myItems = List(0, 1)
    val myLots = List(2, 3)
    val auction = List(4, 5)
}
