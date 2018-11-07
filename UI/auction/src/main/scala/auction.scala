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
            'class /= "columns",
            'div(
              'class /= "column is-8",
              'div(
                'class /= "tile is-ancestor",
                (1 to 5) map {
                  x =>
                    'div(
                      'class /= "tile is-vertical",
                      (1 to 4) map {
                        y =>
                          'div(
                            'class /= "tile is-parent",
                            'div(
                              'class /= "tile is-child",
                              'div(
                                'figure(
                                  'class /= "image is-128x128",
                                  event('click) { access =>
                                    access.transition(_ => MyItems())
                                      .flatMap(_ => access.evalJs("alert('u pwned lol');").map(_ => ()))
                                  },
                                  'img(
                                    'src /= "https://bulma.io/images/placeholders/128x128.png"
                                  )
                                ),
                                'span(
                                  'position @= "relative",
                                  'top @= "-1.5rem",
                                  //x + ", " + y + ": " + ((y - 1) * 5 + x - 1)
                                )
                              )
                            )
                          )
                      }
                    )
                }
              ),
              'br(),
              paginationMyLots(state)
            ),
            'div(
              'class /= "column is-4",
              'div(
                'class /= "notification",
                'backgroundColor @= "#e0e0e0",
                'height @= "100%",
                'width @= "100%",
                'div(
                  'display @= "flex",
                  'flexFlow @= "column",
                  'height @= "100%",
                  'figure(
                    'class /= "image is-4by3",
                    'img(
                      'src /= "https://bulma.io/images/placeholders/640x480.png"
                    )
                  ),
                  'br(),
                  'span(
                    'strong(
                      "Меч судьбы"
                    )
                  ),
                  'span(
                    'strong(
                      "100 GameTokens"
                    )
                  ),
                  'div(
                    'flexGrow @= "1",
                    'position @= "relative",
                    'a(
                      'position @= "absolute",
                      'bottom @= "0",
                      'class /= "button is-warning",
                      'span(
                        "Продать"
                      ),
                      event('click) { access =>
                        access.transition(_ => MyItems(true))
                      }
                    )
                  )
                )
              )
            )
          ),
        ),
        'div(
          'class /= "modal" + (if (state.isSelling) {" is-active"} else {""}),
          'div(
            'class /= "modal-background",
            'backgroundColor @= "rgba(0,0,0,0)"
          ),
          'div(
            'class /= "modal-content",
            'div(
              'class /= "box",
              'article(
                'class /= "media",
                'div(
                  'class /= "media-content",
                  'div(
                    'class /= "content",
                    'div(
                      'span(
                        'strong(
                          "Меч судьбы"
                        )
                      ),
                      'br(),
                      'a(
                        'class /= "button is-warning",
                        'span(
                          "Продать"
                        ),
                        /*event('click) { access =>
                          access.transition(_ => MyItems())
                        }*/
                      )
                    )
                  )
                ),
                'div(
                  'class /= "media-right",
                  'button(
                    'class /= "delete",
                    event('click) { access =>
                      access.transition(_ => MyItems())
                    }
                  )
                )
              )
            )
          ),
          'button(
            'class /= "modal-close is-large",
            'ariaLabel /= "close"
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
              "Мои лоты"
            ),
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
            ),
            'br(),
            'div(
              'class /= "columns",
              'div(
                'class /= "column is-9",
                'div(
                  'class /= "",
                  'div(
                    (1 to 3) map {
                      x =>
                        'div(
                          'class /= "notification",
                          'backgroundColor @= "#e0e0e0",
                          'article(
                            'class /= "media",
                            'figure(
                              'class /= "media-left",
                              'p(
                                'class /= "image is-128x128",
                                'img(
                                  'src /= "https://bulma.io/images/placeholders/256x256.png"
                                )
                              )
                            ),
                            'div(
                              'class /= "media-content",
                              'div(
                                'судьбыclass /= "content",
                                'p(
                                  'strong(
                                    "Меч судьбы"
                                  )
                                )
                              )
                            ),
                            'div(
                              'class /= "media-right",
                              'a(
                                'class /= "button is-danger",
                                'span(
                                  "Удалить"
                                )
                              )
                            )
                          )
                        )
                    }
                  )
                ),
                'br(),
                paginationMyLots(state)
              ),
              'div(
                'class /= "column is-3",
                'div(
                  'class /= "notification",
                  'backgroundColor @= "#e0e0e0",
                  'height @= "100%",
                  "Filter"
                )
              )
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
                'class /= "",
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
              'br(),
              'div(
                'class /= "columns",
                'div(
                  'class /= "column is-9",
                  'div(
                    'class /= "",
                    'div(
                      (1 to 3) map {
                        x =>
                          'div(
                            'class /= "notification",
                            'backgroundColor @= "#e0e0e0",
                            'article(
                              'class /= "media",
                              'figure(
                                'class /= "media-left",
                                'p(
                                  'class /= "image is-128x128",
                                  'img(
                                    'src /= "https://bulma.io/images/placeholders/256x256.png"
                                  )
                                )
                              ),
                              'div(
                                'class /= "media-content",
                                'div(
                                  'class /= "content",
                                  'p(
                                    'strong(
                                      "Меч судьбы"
                                    )
                                  )
                                )
                              ),
                              'div(
                                'class /= "media-right",
                                'a(
                                  'class /= "button is-success",
                                  'span(
                                    "Купить"
                                  ),
                                  event('click) { access =>
                                    access.transition(_ => Auction(true))
                                  }
                                )
                              )
                            )
                          )
                      }
                    )
                  ),
                  'br(),
                  paginationMyLots(state)
                ),
                'div(
                  'class /= "column is-3",
                  'div(
                    'class /= "notification",
                    'backgroundColor @= "#e0e0e0",
                    'height @= "100%",
                    "Filter"
                  )
                )
              )
            ),
            'div(
            'class /= "modal" + (if (state.isBuying) {" is-active"} else {""}),
            'div(
              'class /= "modal-background",
              'backgroundColor @= "rgba(0,0,0,0)"
            ),
            'div(
              'class /= "modal-content",
              'div(
                'class /= "box",
                'article(
                  'class /= "media",
                  'div(
                    'class /= "media-content",
                    'div(
                      'class /= "content",
                      'div(
                        'span(
                          'strong(
                            "Меч судьбы"
                          )
                        ),
                        'br(),
                        'a(
                          'class /= "button is-success",
                          'span(
                            "Купить"
                          ),
                          /*event('click) { access =>
                            access.transition(_ => MyItems())
                          }*/
                        )
                      )
                    )
                  ),
                  'div(
                    'class /= "media-right",
                    'button(
                      'class /= "delete",
                      event('click) { access =>
                        access.transition(_ => Auction())
                      }
                    )
                  )
                )
              )
            ),
            'button(
              'class /= "modal-close is-large",
              'ariaLabel /= "close"
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
              "Мои предметы",
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
              "Мои лоты",
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
              "Аукцион",
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
                'marginRight @= "1rem",
                'span(
                  'strong(
                    "1,234,567 GameTokens"
                  )
                )
              ),
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
        'div(
          'class /= "container",
          content
        )
      )
    )
  }

  private def paginationMyLots(state: GuiState) = {
    'nav(
      'class /= "pagination is-centered",
      'role /= "navigation",
      'ariaLabel /= "pagination",
      'a(
        'class /= "pagination-previous",
        "Previous"
      ),
      'a(
        'class /= "pagination-next",
        "Next page"
      ),
      'ul(
        'class /= "pagination-list",
        'li(
          'a(
            'class /= "pagination-link",
            'ariaLabel /= "Goto page 1",
            "1"
          )
        ),
        'li(
          'span(
            'class /= "pagination-ellipsis",
            "…"
          )
        ),
        'li(
          'a(
            'class /= "pagination-link",
            'ariaLabel /= "Goto page 45",
            "45"
          )
        ),
        'li(
          'a(
            'class /= "pagination-link is-current",
            'ariaLabel /= "Page 46",
            'ariaCurrent /= "page",
            "46"
          )
        ),
        'li(
          'a(
            'class /= "pagination-link",
            'ariaLabel /= "Goto page 47",
            "47"
          )
        ),
        'li(
          'span(
            'class /= "pagination-ellipsis",
            "…"
          )
        ),
        'li(
          'a(
            'class /= "pagination-link",
            'ariaLabel /= "Goto page 86",
            "86"
          )
        )
      )
    )
  }
}
