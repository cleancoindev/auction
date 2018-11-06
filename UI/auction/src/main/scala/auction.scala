import korolev._
import korolev.server._
import korolev.blazeServer._
import korolev.execution._
import korolev.state.javaSerialization._

import scala.concurrent.Future

object SimpleExample extends KorolevBlazeServer {

  import GuiState.globalContext._
  import GuiState.globalContext.symbolDsl._

  val service = blazeService[Future, GuiState, Any] from KorolevServiceConfig[Future, GuiState, Any] (
    head = Seq(
        'link ('href /= "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.1/css/bulma.min.css", 'rel /= "stylesheet"),
        'link (
          'href        /= "https://use.fontawesome.com/releases/v5.0.13/css/all.css",
          'rel         /= "stylesheet",
          'crossorigin /= "anonymous",
          'integrity   /= "sha384-DNOHZ68U8hZfKXOrtjWvjxusGo9WQnrNx2sqG0tfsghAvtVlRW3tvkXWZh58N9jp"
        )
        //'link ('href /= "main.css", 'rel /= "stylesheet")
      ),
    stateStorage = StateStorage.default(MyItems()),
    render = {
      case state: MyItems =>
        mainLayout(state,   
          'div(
            'class /= "notification",
            'height @= "100%",
            'h3(
              'class /= "title is-3",
              "Мои предметы"
            ),
            'div(
              'class /= "tile is-ancestor",
              (1 to 4) map {
                x =>
                  'div(
                    'class /= "tile is-3 is-vertical",
                    (1 to 4) map {
                      y =>
                        'div(
                          'class /= "tile is-3 is-parent",
                          'div(
                            'class /= "tile is-child",
                            event('mouseover) { access =>
                              access.transition(_ => MyItems())
                                .flatMap(_ => access.evalJs("console.log('u pwned lol');").map(_ => ()))
                            },
                            'figure(
                              'class /= "image is-128x128",
                              'img(
                                'src /= "https://bulma.io/images/placeholders/128x128.png"
                              )
                            )
                          )
                        )
                    }
                  )
              }
            )
          )
        )
      case state: MyLots =>
        mainLayout(state,   
          'div(
            'class /= "notification",
            'height @= "100%",
            'h3(
              'class /= "title is-3",
              "Мои предметы"
            ),
            'div(
              'class /= "tile is-ancestor",
              (1 to 4) map {
                x =>
                  'div(
                    'class /= "tile is-3 is-vertical",
                    (1 to 4) map {
                      y =>
                        'div(
                          'class /= "tile is-3 is-parent",
                          'div(
                            'class /= "tile is-child",
                            event('mouseover) { access =>
                              access.transition(_ => MyItems())
                                .flatMap(_ => access.evalJs("console.log('u pwned lol');").map(_ => ()))
                            },
                            'figure(
                              'class /= "image is-128x128",
                              'img(
                                'src /= "https://bulma.io/images/placeholders/128x128.png"
                              )
                            )
                          )
                        )
                    }
                  )
              }
            )
          )
        )
      case state: Auction =>
        mainLayout(state,
          'div(
            'class /= "notification",
            'height @= "100%",
            'h3(
              'class /= "title is-3",
              "Торговая площадка"
            ),
            'div(
              'class /= "notification is-primary",
              'p(
                'class /= "control has-icons-left",
                'input(
                  'class /= "input is-small",
                  'type /= "text",
                  'placeholder /= "search"
                ),
                'span(
                  'class /= "icon is-small is-left",
                  'i(
                    'class /= "fas fa-search",
                    'ariaHidden /= "true"
                  )
                )
              )
            ),
            'div(
              'class /= "columns",
              'div(
                'class /= "column is-9",
                'div(
                  'class /= "notification is-info",
                  "Heeeeeeeeey"
                )
              ),
              'div(
                'class /= "column is-3",
                'div(
                  'class /= "notification is-info",
                  "11er"
                )
              )
            )
          )
        )
    },
    router = emptyRouter
  )


  private def mainLayout(state: GuiState, content: Node*) = {
    'body(
      'nav(
        'class /= "navbar",
        'role /= "navigation",
        'ariaLabel /= "main navigation",
        'div(
          'class /= "navbar-brand",
          'a(
            'class /= "navbar-item",
            'p(
              'fontWeight @= "600",
              'fontSize @= "2 rem",
              "EXPLOAD"
            )
          ),
          'a(
            'role /= "button",
            'class /= "navbar-burger burger",
            'ariaLabel /= "menu",
            'ariaExpanded /= "false",
            'dataTarget /= "navbarBasicExample",
            'span(
              'ariaHidden /= "true"
            ),
            'span(
              'ariaHidden /= "true"
            ),
            'span(
              'ariaHidden /= "true"
            )
          )
        ),
        'div(
          'id /= "navbar",
          'class /= "navbar-menu",
          'div(
            'class /= "navbar-start",
            'a(
              'class /= (state match {
                case state: MyItems =>
                  "navbar-item is active"
                case _ =>
                  "navbar-item"
              }),
              "My Items",
              event('click) { access =>
                access.transition(_ => MyItems())
              }
            ),
            'a(
              'class /= (state match {
                case state: MyLots =>
                  "navbar-item is active"
                case _ =>
                  "navbar-item"
              }),
              "My Lots",
              event('click) { access =>
                access.transition(_ => MyLots())
              }
            ),
            'a(
              'class /= (state match {
                case state: Auction =>
                  "navbar-item is active"
                case _ =>
                  "navbar-item"
              }),
              "Auction",
              event('click) { access =>
                access.transition(_ => Auction())
              }
            )
          ),
          'div(
            'class /= "navbar-end",
            'div(
              'class /= "navbar-item",
              'div(
                'class /= "buttons",
                'a(
                  'class /= "button is-primary",
                  'strong(
                    "Sign up"
                  )
                ),
                'a(
                  'class /= "button is-light",
                  "Log in"
                )
              )
            )
          )
        )
      ),
      'section(
        'class /= "section",
        content
      )
    )
  }
}
