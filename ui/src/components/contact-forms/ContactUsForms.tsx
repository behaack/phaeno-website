import {
  GoogleReCaptchaProvider,
} from "react-google-recaptcha-v3";
import { ContactForm } from "./ContactForm";
import { OrderForm } from "./OrderForm";

const CAPTCHA_SITE_KEY = import.meta.env.PUBLIC_RECAPTCHA_SITE_ID;

export default function ContactUsForms() {
  return (
    <GoogleReCaptchaProvider
      reCaptchaKey={CAPTCHA_SITE_KEY}
      scriptProps={{ async: true, defer: true }}
    >
      <section className="demo-band" aria-labelledby="request-demo">
        <div className="demo-band__inner">
          <h2
            id="request-demo"
            className="pb-2"
            style={{ scrollMarginTop: "7rem" }}
            data-phaeno-search="Request a PSeq demo"
            data-phaeno-search-summary="Request a PSeq demo to walk through isoform-resolved RNA data with the Phaeno team."
            data-phaeno-search-keywords="PSeq demo request isoform-resolved RNA data sample project"
          >See PSeq in Action</h2>
          <p>Walk through isoform-resolved RNA data with our team using your sample.</p>
          <OrderForm />
        </div>
      </section>
      <h2
        id="sign-up"
        className="mt-4 pb-2"
        style={{ scrollMarginTop: "7rem" }}
        data-phaeno-search="Get Phaeno updates and the PSeq technical brief"
        data-phaeno-search-summary="Sign up for Phaeno product releases, validation updates, technical insights, and the PSeq technical brief."
        data-phaeno-search-keywords="Phaeno updates PSeq technical brief validation updates product releases"
      >Stay Updated on Phaeno</h2>
      <p>Product releases, validation updates, and technical insights. No spam.</p>
      <ContactForm />
    </GoogleReCaptchaProvider>
  );
}
